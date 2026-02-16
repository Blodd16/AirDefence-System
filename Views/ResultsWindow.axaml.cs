using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AirDefenseSimulation.Models;
using System;
using System.Linq;
using Avalonia.Controls.Shapes;

namespace AirDefenseSimulation.Views;

public partial class ResultsWindow : Window
{

    public bool StartNewSimulation { get; private set; }
        private SimulationResults? _results;

    public ResultsWindow()
    {
        InitializeComponent();
        Opened += (_, __) =>
        {
            if (_results?.TimelineData != null)
            {
                DrawTimeline(_results.TimelineData);
                FillEvents(_results.TimelineData);
            }
        };
        InitializeComponent();
    }

    public void SetResults(SimulationResults results)
    {
        _results = results;
        // Calculate statistics
        var survivalRate = (results.Survivors * 100.0 / results.InitialPopulation);
        var interceptionRate = results.MissilesFired > 0
            ? (results.SuccessfulInterceptions * 100.0 / results.MissilesFired)
            : 0;

        // Calculate score (0-100)
        var score = (survivalRate * 0.4) + (results.CityHealthPercentage * 0.3) + (interceptionRate * 0.3);

        // Update Score Section
        this.FindControl<ProgressBar>("FinalScoreBar")!.Value = score;
        this.FindControl<TextBlock>("FinalScoreText")!.Text = $"{score:F0} / 100 Points";

        // Rating with color
        var ratingText = this.FindControl<TextBlock>("FinalRating")!;
        if (score >= 90)
        {
            ratingText.Text = "⭐⭐⭐⭐⭐ EXCELLENT!";
            ratingText.Foreground = new SolidColorBrush(Color.Parse("#fbbf24"));
        }
        else if (score >= 75)
        {
            ratingText.Text = "⭐⭐⭐⭐ VERY GOOD";
            ratingText.Foreground = new SolidColorBrush(Color.Parse("#10b981"));
        }
        else if (score >= 60)
        {
            ratingText.Text = "⭐⭐⭐ GOOD DEFENSE";
            ratingText.Foreground = new SolidColorBrush(Color.Parse("#06b6d4"));
        }
        else if (score >= 40)
        {
            ratingText.Text = "⭐⭐ AVERAGE";
            ratingText.Foreground = new SolidColorBrush(Color.Parse("#f59e0b"));
        }
        else
        {
            ratingText.Text = "⭐ NEEDS IMPROVEMENT";
            ratingText.Foreground = new SolidColorBrush(Color.Parse("#dc2626"));
        }

        // Update City Statistics
        this.FindControl<TextBlock>("TxtInitialPop")!.Text = results.InitialPopulation.ToString();
        this.FindControl<TextBlock>("TxtSurvivors")!.Text = results.Survivors.ToString();
        this.FindControl<TextBlock>("TxtCasualties")!.Text = results.Casualties.ToString();
        this.FindControl<TextBlock>("TxtInBunkers")!.Text = results.PeopleInBunkers.ToString();
        this.FindControl<TextBlock>("TxtEvacuated")!.Text = results.TotalEvacuated.ToString();
        this.FindControl<TextBlock>("TxtBuildingsDestroyed")!.Text = results.BuildingsDestroyed.ToString();
        this.FindControl<TextBlock>("TxtCityHealth")!.Text = $"{results.CityHealthPercentage:F0}%";

        // Update Defense Performance
        this.FindControl<TextBlock>("TxtMissilesFired")!.Text = results.MissilesFired.ToString();
        this.FindControl<TextBlock>("TxtInterceptions")!.Text = results.SuccessfulInterceptions.ToString();
        this.FindControl<TextBlock>("TxtMissed")!.Text = results.ThreatsMissed.ToString();
        this.FindControl<TextBlock>("TxtInterceptionRate")!.Text = $"{interceptionRate:F1}%";
        this.FindControl<TextBlock>("TxtDuration")!.Text = results.Duration;
        this.FindControl<TextBlock>("TxtTotalThreats")!.Text = results.TotalThreats.ToString();

        // Update Threat Breakdown
        this.FindControl<TextBlock>("TxtFighters")!.Text = results.EnemyFighters.ToString();
        this.FindControl<TextBlock>("TxtMissiles")!.Text = results.EnemyMissiles.ToString();
        this.FindControl<TextBlock>("TxtDrones")!.Text = results.EnemyDrones.ToString();
    }

    private void DrawTimeline(TimelineTracker tracker)
    {
        var canvas = this.FindControl<Canvas>("TimelineCanvas")!;
        canvas.Children.Clear();

        if (tracker.Snapshots.Count < 2)
        {
            canvas.Children.Add(new TextBlock
            {
                Text = "No timeline data",
                Foreground = Brushes.Gray,
                FontSize = 12
            });
            return;
        }

        double w = canvas.Bounds.Width > 0 ? canvas.Bounds.Width : 550;
        double h = canvas.Bounds.Height > 0 ? canvas.Bounds.Height : 190;

        double maxTime = tracker.Snapshots[^1].Time;
        if (maxTime <= 0) maxTime = 1;

        int maxThreats = Math.Max(1, tracker.Snapshots.Max(s => s.ActiveThreats));
        int maxInter = Math.Max(1, tracker.Snapshots.Max(s => s.TotalInterceptions));
        int maxCas = Math.Max(1, tracker.Snapshots.Max(s => s.TotalCasualties));

        var threatsLine = new Polyline
        {
            Stroke = new SolidColorBrush(Color.Parse("#ef4444")),
            StrokeThickness = 2
        };

        var interLine = new Polyline
        {
            Stroke = new SolidColorBrush(Color.Parse("#10b981")),
            StrokeThickness = 2
        };

        var casLine = new Polyline
        {
            Stroke = new SolidColorBrush(Color.Parse("#8b5cf6")),
            StrokeThickness = 2
        };

        var healthLine = new Polyline
        {
            Stroke = new SolidColorBrush(Color.Parse("#06b6d4")),
            StrokeThickness = 2
        };

        foreach (var s in tracker.Snapshots)
        {
            double x = (s.Time / maxTime) * (w - 10) + 5;

            double yThreats = h - (s.ActiveThreats / (double)maxThreats) * (h - 10) - 5;
            double yInter = h - (s.TotalInterceptions / (double)maxInter) * (h - 10) - 5;
            double yCas = h - (s.TotalCasualties / (double)maxCas) * (h - 10) - 5;
            double yHealth = h - (s.CityHealth / 100.0) * (h - 10) - 5;

            threatsLine.Points.Add(new Avalonia.Point(x, yThreats));
            interLine.Points.Add(new Avalonia.Point(x, yInter));
            casLine.Points.Add(new Avalonia.Point(x, yCas));
            healthLine.Points.Add(new Avalonia.Point(x, yHealth));
        }

        canvas.Children.Add(threatsLine);
        canvas.Children.Add(interLine);
        canvas.Children.Add(casLine);
        canvas.Children.Add(healthLine);
    }

    private void FillEvents(TimelineTracker tracker)
    {
        var panel = this.FindControl<StackPanel>("EventsListPanel")!;
        panel.Children.Clear();

        var key = tracker.GetKeyMoments().OrderBy(e => e.Time).Take(60).ToList();

        if (key.Count == 0)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "No events recorded",
                Foreground = Brushes.Gray,
                FontSize = 12,
                FontStyle = FontStyle.Italic
            });
            return;
        }

        foreach (var ev in key)
        {
            panel.Children.Add(new TextBlock
            {
                Text = $"{TimelineTracker.FormatTime(ev.Time)} - {ev.Description}",
                Foreground = Brushes.White,
                FontSize = 12
            });
        }
    }

    private void OnNewSimulationClick(object? sender, RoutedEventArgs e)
    {
        StartNewSimulation = true;
        Close();
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        StartNewSimulation = false;
        Close();
    }
}

// Data class to hold simulation results
public class SimulationResults
{
    // City Stats
    public int InitialPopulation { get; set; }
    public int Survivors { get; set; }
    public int Casualties { get; set; }
    public int PeopleInBunkers { get; set; }
    public int TotalEvacuated { get; set; }
    public int BuildingsDestroyed { get; set; }
    public double CityHealthPercentage { get; set; }

    // Defense Performance
    public int MissilesFired { get; set; }
    public int SuccessfulInterceptions { get; set; }
    public int ThreatsMissed { get; set; }
    public string Duration { get; set; } = "0:00";
    public int TotalThreats { get; set; }


    // Enemy Breakdown
    public int EnemyFighters { get; set; }
    public int EnemyMissiles { get; set; }
    public int EnemyDrones { get; set; }

    // Timeline Data
    public TimelineTracker? TimelineData { get; set; }
}