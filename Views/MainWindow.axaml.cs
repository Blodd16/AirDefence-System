using AirDefenseSimulation.Helpers;
using AirDefenseSimulation.Models;
using AirDefenseSimulation.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AirDefenseSimulation.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly DispatcherTimer _timer;
    private DateTime _lastUpdate;

    private City? _city;
    private DefenseSystem? _defenseSystem;
    private readonly List<AirObject> _enemies = new();

    // Timeline tracking
    private DateTime _simulationStartTime;
    private bool _simulationEnded = false;
    private TimelineTracker _timeline = new TimelineTracker();

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        DataContext = _viewModel;

        _lastUpdate = DateTime.Now;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _timer.Tick += OnTimerTick;

        // Получаем ссылки на элементы после InitializeComponent
        this.Loaded += (s, e) =>
        {
            ChartCanvas = this.FindControl<Canvas>("ChartCanvas");
            ChartTypeCombo = this.FindControl<ComboBox>("ChartTypeCombo");
        };

        InitializeSimulation();
    }

    private void InitializeSimulation()
    {
        var centerX = 450.0;
        var centerY = 300.0;

        _city = new City(new Vector2D(centerX, centerY), 150, _viewModel.PopulationCount);
        _defenseSystem = new DefenseSystem(
            new Vector2D(centerX, centerY),
            _viewModel.DetectionRadius,
            _viewModel.InterceptionRadius);

        _enemies.Clear();
        SpawnEnemies();

        _viewModel.TotalSpawned = _enemies.Count;
        _viewModel.ActiveObjects = _enemies.Count;
        _viewModel.DestroyedObjects = 0;
        _viewModel.MissilesFired = 0;
        _viewModel.SuccessfulInterceptions = 0;
        _viewModel.ElapsedTime = 0;
        _viewModel.EvacuatedCount = 0;
        _viewModel.CasualtiesCount = 0;

        // Сброс timeline
        _timeline.Clear();
        _simulationEnded = false;

        if (SimulationCanvas != null)
        {
            SimulationCanvas.City = _city;
            SimulationCanvas.DefenseSystem = _defenseSystem;
            SimulationCanvas.Enemies = _enemies;
        }
    }

    private void SpawnEnemies()
    {
        var centerX = 450.0;
        var centerY = 300.0;
        var spawnRadius = 400.0;

        var distType = DistributionCombo?.SelectedIndex switch
        {
            1 => DistributionType.Uniform,
            2 => DistributionType.Exponential,
            _ => DistributionType.Normal
        };

        for (int i = 0; i < _viewModel.EnemyFighterCount; i++)
        {
            var angle = (i / (double)_viewModel.EnemyFighterCount) * Math.PI * 2;
            var startPos = new Vector2D(
                centerX + Math.Cos(angle) * spawnRadius,
                centerY + Math.Sin(angle) * spawnRadius);

            var speed = Distribution.Generate(distType, 100, 20);
            var target = new Vector2D(
                centerX + Distribution.Normal(0, 30),
                centerY + Distribution.Normal(0, 30));

            _enemies.Add(new EnemyFighter($"fighter-{i}", startPos, target, Math.Max(50, speed)));
        }

        for (int i = 0; i < _viewModel.EnemyMissileCount; i++)
        {
            var angle = (i / (double)_viewModel.EnemyMissileCount) * Math.PI * 2;
            var startPos = new Vector2D(
                centerX + Math.Cos(angle) * (spawnRadius + 50),
                centerY + Math.Sin(angle) * (spawnRadius + 50));

            var speed = Distribution.Generate(distType, 150, 30);
            var target = new Vector2D(centerX, centerY);

            _enemies.Add(new EnemyMissile($"missile-{i}", startPos, target, Math.Max(100, speed)));
        }

        for (int i = 0; i < _viewModel.EnemyDroneCount; i++)
        {
            var side = i % 4;
            Vector2D startPos;

            if (side == 0)
                startPos = new Vector2D(Distribution.Uniform(50, 850), 30);
            else if (side == 1)
                startPos = new Vector2D(870, Distribution.Uniform(50, 550));
            else if (side == 2)
                startPos = new Vector2D(Distribution.Uniform(50, 850), 570);
            else
                startPos = new Vector2D(30, Distribution.Uniform(50, 550));

            var speed = Distribution.Uniform(40, 80);
            var target = new Vector2D(
                centerX + Distribution.Normal(0, 40),
                centerY + Distribution.Normal(0, 40));

            _enemies.Add(new EnemyDrone($"drone-{i}", startPos, target, speed));
        }
    }

    // Neue Methode für Zeitformatierung hinzufügen:
    private void UpdateElapsedTimeDisplay()
    {
        var totalSeconds = (int)_viewModel.ElapsedTime;
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        _viewModel.ElapsedTimeFormatted = $"{minutes:D2}:{seconds:D2}";
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_viewModel.IsRunning) return;

        var now = DateTime.Now;
        var deltaTime = (now - _lastUpdate).TotalSeconds * _viewModel.SimulationSpeed;
        _lastUpdate = now;

        if (deltaTime > 0.1) deltaTime = 0.1;

        // Сохраняем предыдущие значения для отслеживания изменений
        var prevInterceptions = _viewModel.SuccessfulInterceptions;
        var prevCasualties = _viewModel.CasualtiesCount;
        var prevBuildings = _viewModel.BuildingsDestroyed;

        foreach (var enemy in _enemies)
        {
            enemy.Update(deltaTime);
        }

        if (_city != null)
        {
            _viewModel.EvacuatedCount = _city.EvacuatedCount;
            _viewModel.CasualtiesCount = _city.CasualtiesCount;
            _viewModel.PeopleInBunkers = _city.PeopleInBunkers;
            _viewModel.BuildingsDestroyed = _city.BuildingsDestroyed;
            _viewModel.CityHealthPercentage = _city.CityHealthPercentage;
            _viewModel.ThreatsMissed = _city.ThreatsMissed;
        }

        _city?.Update(deltaTime);
        _defenseSystem?.Update(deltaTime, _enemies, _city);

        _viewModel.PeopleInBunkers = _city?.PeopleInBunkers ?? 0;
        _viewModel.ActiveObjects = _enemies.Count(e => e.IsActive);
        _viewModel.DestroyedObjects = _enemies.Count(e => e.IsDestroyed);
        _viewModel.MissilesFired = _defenseSystem?.MissilesLaunched ?? 0;
        _viewModel.SuccessfulInterceptions = _defenseSystem?.SuccessfulInterceptions ?? 0;
        _viewModel.ElapsedTime += deltaTime;

        UpdateElapsedTimeDisplay();

        // Отслеживание событий для Timeline
        var currentTime = _viewModel.ElapsedTime;


        // Перехваты
        if (_viewModel.SuccessfulInterceptions > prevInterceptions)
        {
            var count = _viewModel.SuccessfulInterceptions - prevInterceptions;
            _timeline.AddEvent(currentTime, EventType.Interception,
                $"Intercepted {count} threat{(count > 1 ? "s" : "")}", count);
        }

        // Жертвы
        if (_viewModel.CasualtiesCount > prevCasualties)
        {
            var count = _viewModel.CasualtiesCount - prevCasualties;
            _timeline.AddEvent(currentTime, EventType.Casualties,
                $"{count} casualt{(count > 1 ? "ies" : "y")}", count);
        }

        // Здания
        if (_viewModel.BuildingsDestroyed > prevBuildings)
        {
            var count = _viewModel.BuildingsDestroyed - prevBuildings;
            _timeline.AddEvent(currentTime, EventType.BuildingDestroyed,
                $"{count} building{(count > 1 ? "s" : "")} destroyed", count);
        }

        // Попадания в город
        if (_city != null && _city.ThreatsMissed > 0)
        {
            var recentHits = _city.Explosions.Count(exp => !exp.WasIntercepted && exp.Life > 0.8);
            if (recentHits > 0)
            {
                _timeline.AddEvent(currentTime, EventType.CityHit,
                    "City hit by missile!", recentHits);
            }
        }

        // Снимок состояния (каждую секунду)
        if (_city != null)
        {
            _timeline.TakeSnapshot(
                currentTime,
                _viewModel.ActiveObjects,
                _viewModel.SuccessfulInterceptions,
                _viewModel.CasualtiesCount,
                _city.CityHealthPercentage,
                _city.PeopleInBunkers
            );
        }

        UpdateChart();
        SimulationCanvas?.InvalidateVisual();

        // Проверка на конец симуляции
        CheckSimulationEnd();
    }

    private void CheckSimulationEnd()
    {
        if (_simulationEnded) return;

        var activeThreats = _enemies.Count(e => e.IsActive);

        // Условия окончания симуляции
        bool allThreatsDestroyed = activeThreats == 0 && _enemies.Count > 0;
        bool cityDestroyed = _city != null && _city.CityHealthPercentage <= 10;

        if (allThreatsDestroyed || cityDestroyed)
        {
            _simulationEnded = true;
            _viewModel.IsRunning = false;
            _timer.Stop();

            if (PlayPauseButton != null)
            {
                PlayPauseButton.Content = "Start";
            }

            // Показываем финальное окно результатов
            ShowFinalResults();
        }
    }

    private async void ShowFinalResults()
    {
        if (_city == null || _defenseSystem == null) return;

        var duration = DateTime.Now - _simulationStartTime;
        var durationText = $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}";

        var results = new SimulationResults
        {
            // City Stats
            InitialPopulation = _city.InitialPopulation,
            Survivors = _city.People.Count + _city.PeopleInBunkers,
            Casualties = _city.CasualtiesCount,
            PeopleInBunkers = _city.PeopleInBunkers,
            TotalEvacuated = _city.EvacuatedCount + _city.PeopleInBunkers,
            BuildingsDestroyed = _city.BuildingsDestroyed,
            CityHealthPercentage = _city.CityHealthPercentage,

            // Defense Performance
            MissilesFired = _viewModel.MissilesFired,
            SuccessfulInterceptions = _viewModel.SuccessfulInterceptions,
            ThreatsMissed = _city.ThreatsMissed,
            Duration = durationText,
            TotalThreats = _viewModel.EnemyFighterCount + _viewModel.EnemyMissileCount + _viewModel.EnemyDroneCount,

            // Enemy Breakdown
            EnemyFighters = _viewModel.EnemyFighterCount,
            EnemyMissiles = _viewModel.EnemyMissileCount,
            EnemyDrones = _viewModel.EnemyDroneCount,

            // Timeline Data
            TimelineData = _timeline
        };

        var resultsWindow = new ResultsWindow();
        resultsWindow.SetResults(results);

        await resultsWindow.ShowDialog(this);

        // Если пользователь нажал "New Simulation"
        if (resultsWindow.StartNewSimulation)
        {
            OnResetClick(null, new RoutedEventArgs());
        }
    }

    private void OnChartTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateChart();
    }

    private void OnPlayPauseClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _viewModel.IsRunning = !_viewModel.IsRunning;

        if (PlayPauseButton != null)
        {
            PlayPauseButton.Content = _viewModel.IsRunning ? "Pause" : "Start";
        }

        if (_viewModel.IsRunning)
        {
            if (!_simulationEnded)
            {
                _simulationStartTime = DateTime.Now;
            }
            _lastUpdate = DateTime.Now;
            _timer.Start();
        }
        else
        {
            _timer.Stop();
        }
    }

    private void HideResults()
    {
        var resultsPanel = this.FindControl<Border>("ResultsPanel");
        if (resultsPanel != null)
        {
            resultsPanel.IsVisible = false;
        }
    }

    private void OnResetClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // stop simulation
        _viewModel.IsRunning = false;
        _timer.Stop();
        _simulationEnded = false;

        if (PlayPauseButton != null)
        {
            PlayPauseButton.Content = "Start";
        }

        // clean up enemies
        _enemies.Clear();

        // Очистить timeline
        _timeline.Clear();

        // Переинициализация города и системы ПВО
        var centerX = 450.0;
        var centerY = 300.0;

        _city = new City(new Vector2D(centerX, centerY), 150, _viewModel.PopulationCount);
        _defenseSystem = new DefenseSystem(
            new Vector2D(centerX, centerY),
            _viewModel.DetectionRadius,
            _viewModel.InterceptionRadius);

        SpawnEnemies();

        // Привязка к канвасу симуляции
        if (SimulationCanvas != null)
        {
            SimulationCanvas.City = _city;
            SimulationCanvas.DefenseSystem = _defenseSystem;
            SimulationCanvas.Enemies = _enemies;
            SimulationCanvas.InvalidateVisual();
        }

        // all the counters to zero
        _viewModel.TotalSpawned = 0;
        _viewModel.ActiveObjects = 0;
        _viewModel.DestroyedObjects = 0;
        _viewModel.MissilesFired = 0;
        _viewModel.SuccessfulInterceptions = 0;
        _viewModel.ElapsedTime = 0;
        _viewModel.EvacuatedCount = 0;
        _viewModel.CasualtiesCount = 0;
        _viewModel.PeopleInBunkers = 0;
        _viewModel.CityHealthPercentage = 100;
        _viewModel.ThreatsMissed = 0;
        _viewModel.ElapsedTime = 0;
        _viewModel.ElapsedTimeFormatted = "00:00";
        UpdateChart();
        HideResults();
    }

    private void OnContinueClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        HideResults();
        _viewModel.IsRunning = true;
        if (PlayPauseButton != null)
        {
            PlayPauseButton.Content = "Pause";
        }
        _lastUpdate = DateTime.Now;
        _timer.Start();
    }
    private static decimal GetSafeValue(NumericUpDown control, decimal defaultValue)
    {
        if (control == null)
            return defaultValue;

        if (control.Value == null)
        {
            control.Value = defaultValue;
            return defaultValue;
        }

        return control.Value.Value;
    }

    private void OnApplySettingsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Hole alle NumericUpDown Controls
        var fightersControl = this.FindControl<NumericUpDown>("FightersUpDown");
        var missilesControl = this.FindControl<NumericUpDown>("MissilesUpDown");
        var dronesControl = this.FindControl<NumericUpDown>("DronesUpDown");
        var populationControl = this.FindControl<NumericUpDown>("PopulationUpDown");
        var capacityControl = this.FindControl<NumericUpDown>("CapacityUpDown");
        var detectionControl = this.FindControl<NumericUpDown>("DetectionUpDown");
        var interceptionControl = this.FindControl<NumericUpDown>("InterceptionUpDown");

        // Validiere alle Felder (korrigiert automatisch ungültige Werte)
        bool isValid = ValidationHelper.ValidateAllFields(
            fightersControl,
            missilesControl,
            dronesControl,
            populationControl,
            capacityControl,
            detectionControl,
            interceptionControl);

        // Warte kurz wenn Fehler korrigiert wurden, damit der Benutzer die Korrektur sieht
        if (!isValid)
        {
            // Nach Korrektur trotzdem fortfahren
            System.Threading.Thread.Sleep(500);
        }

        // Aktualisiere ViewModel mit den (möglicherweise korrigierten) Werten

        _viewModel.EnemyFighterCount =
    (       int)GetSafeValue(fightersControl, 0);

        _viewModel.EnemyMissileCount =
            (int)GetSafeValue(missilesControl, 0);

        _viewModel.EnemyDroneCount =
            (int)GetSafeValue(dronesControl, 0);

        _viewModel.PopulationCount =
            (int)GetSafeValue(populationControl, 10);

        _viewModel.DefenseRocketCapacity =
            (int)GetSafeValue(capacityControl, 5);

        _viewModel.DetectionRadius =
            (double)GetSafeValue(detectionControl, 100);

        _viewModel.InterceptionRadius =
            (double)GetSafeValue(interceptionControl, 50);

        _viewModel.IsRunning = false;
        _timer.Stop();

        if (PlayPauseButton != null)
        {
            PlayPauseButton.Content = "Start";
        }

        InitializeSimulation();
        SimulationCanvas?.InvalidateVisual();
        UpdateChart();
    }

    private void UpdateChart()
    {
        if (ChartCanvas == null || _city == null || _defenseSystem == null) return;

        var canvas = ChartCanvas;
        canvas.Children.Clear();

        var intercepted = _defenseSystem.SuccessfulInterceptions;
        var hitCity = _viewModel.DestroyedObjects - intercepted;
        var evacuated = _city.EvacuatedCount;
        var casualties = _city.CasualtiesCount;

        var isPieChart = ChartTypeCombo?.SelectedIndex == 1;

        if (isPieChart)
        {
            DrawPieChart(canvas, intercepted, Math.Max(0, hitCity), evacuated, casualties);
        }
        else
        {
            DrawBarChart(canvas, intercepted, Math.Max(0, hitCity), evacuated, casualties);
        }
    }

    private void DrawBarChart(Canvas canvas, int val1, int val2, int val3, int val4)
    {
        var maxVal = Math.Max(Math.Max(val1, val2), Math.Max(val3, val4));
        if (maxVal == 0) maxVal = 1;

        var barWidth = 50.0;
        var spacing = 15.0;
        var maxHeight = 150.0;
        var startX = 20.0;
        var startY = 170.0;

        // Bar 1 - Intercepted
        var height1 = (val1 / (double)maxVal) * maxHeight;
        var rect1 = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = barWidth,
            Height = height1,
            Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129))
        };
        Canvas.SetLeft(rect1, startX);
        Canvas.SetTop(rect1, startY - height1);
        canvas.Children.Add(rect1);

        var text1 = new TextBlock
        {
            Text = val1.ToString(),
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };
        Canvas.SetLeft(text1, startX + barWidth / 2 - 8);
        Canvas.SetTop(text1, startY - height1 - 20);
        canvas.Children.Add(text1);

        // Bar 2 - Hit City
        var height2 = (val2 / (double)maxVal) * maxHeight;
        var rect2 = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = barWidth,
            Height = height2,
            Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68))
        };
        Canvas.SetLeft(rect2, startX + barWidth + spacing);
        Canvas.SetTop(rect2, startY - height2);
        canvas.Children.Add(rect2);

        var text2 = new TextBlock
        {
            Text = val2.ToString(),
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };
        Canvas.SetLeft(text2, startX + barWidth + spacing + barWidth / 2 - 8);
        Canvas.SetTop(text2, startY - height2 - 20);
        canvas.Children.Add(text2);

        // Bar 3 - Evacuated
        var height3 = (val3 / (double)maxVal) * maxHeight;
        var rect3 = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = barWidth,
            Height = height3,
            Fill = new SolidColorBrush(Color.FromRgb(245, 158, 11))
        };
        Canvas.SetLeft(rect3, startX + (barWidth + spacing) * 2);
        Canvas.SetTop(rect3, startY - height3);
        canvas.Children.Add(rect3);

        var text3 = new TextBlock
        {
            Text = val3.ToString(),
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };
        Canvas.SetLeft(text3, startX + (barWidth + spacing) * 2 + barWidth / 2 - 8);
        Canvas.SetTop(text3, startY - height3 - 20);
        canvas.Children.Add(text3);

        // Bar 4 - Casualties
        var height4 = (val4 / (double)maxVal) * maxHeight;
        var rect4 = new Avalonia.Controls.Shapes.Rectangle
        {
            Width = barWidth,
            Height = height4,
            Fill = new SolidColorBrush(Color.FromRgb(139, 92, 246))
        };
        Canvas.SetLeft(rect4, startX + (barWidth + spacing) * 3);
        Canvas.SetTop(rect4, startY - height4);
        canvas.Children.Add(rect4);

        var text4 = new TextBlock
        {
            Text = val4.ToString(),
            Foreground = Brushes.White,
            FontSize = 12,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };
        Canvas.SetLeft(text4, startX + (barWidth + spacing) * 3 + barWidth / 2 - 8);
        Canvas.SetTop(text4, startY - height4 - 20);
        canvas.Children.Add(text4);
    }

    private void DrawPieChart(Canvas canvas, int val1, int val2, int val3, int val4)
    {
        var total = val1 + val2 + val3 + val4;
        if (total == 0) total = 1;

        var centerX = 135.0;
        var centerY = 100.0;
        var radius = 70.0;

        var colors = new[]
        {
            Color.FromRgb(16, 185, 129),  // Green
            Color.FromRgb(239, 68, 68),   // Red
            Color.FromRgb(245, 158, 11),  // Orange
            Color.FromRgb(139, 92, 246)   // Purple
        };

        var values = new[] { val1, val2, val3, val4 };
        var startAngle = -90.0;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] == 0) continue;

            var sweepAngle = (values[i] / (double)total) * 360.0;

            var path = new Avalonia.Controls.Shapes.Path
            {
                Fill = new SolidColorBrush(colors[i]),
                Stroke = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                StrokeThickness = 2
            };

            var geometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = new Point(centerX, centerY) };

            var startRad = startAngle * Math.PI / 180.0;
            var endRad = (startAngle + sweepAngle) * Math.PI / 180.0;

            var startPoint = new Point(
                centerX + radius * Math.Cos(startRad),
                centerY + radius * Math.Sin(startRad));

            var endPoint = new Point(
                centerX + radius * Math.Cos(endRad),
                centerY + radius * Math.Sin(endRad));

            figure.Segments = new PathSegments
            {
                new LineSegment { Point = startPoint },
                new ArcSegment
                {
                    Point = endPoint,
                    Size = new Size(radius, radius),
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = sweepAngle > 180
                }
            };

            figure.IsClosed = true;
            geometry.Figures.Add(figure);
            path.Data = geometry;

            canvas.Children.Add(path);

            // Percentage text
            var midAngle = (startAngle + sweepAngle / 2) * Math.PI / 180.0;
            var textX = centerX + (radius * 0.6) * Math.Cos(midAngle);
            var textY = centerY + (radius * 0.6) * Math.Sin(midAngle);

            var percentage = (values[i] / (double)total) * 100;
            if (percentage > 5)
            {
                var text = new TextBlock
                {
                    Text = $"{percentage:F0}%",
                    Foreground = Brushes.White,
                    FontSize = 11,
                    FontWeight = Avalonia.Media.FontWeight.Bold
                };
                Canvas.SetLeft(text, textX - 15);
                Canvas.SetTop(text, textY - 8);
                canvas.Children.Add(text);
            }

            startAngle += sweepAngle;
        }
    }
}