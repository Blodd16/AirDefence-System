using AirDefenseSimulation.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AirDefenseSimulation.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private bool _isRunning;
    private double _simulationSpeed = 1.0;
    private int _enemyFighterCount = 5;
    private int _enemyMissileCount = 8;
    private int _enemyDroneCount = 5;
    private double _detectionRadius = 250;
    private double _interceptionRadius = 150;
    private int _populationCount = 100;
    private int _totalSpawned;
    private int _activeObjects;
    private int _destroyedObjects;
    private int _missilesFired;
    private int _successfulInterceptions;
    private double _elapsedTime;
    private int _evacuatedCount;
    private int _casualtiesCount;
    private double _missileSpeed = 150;
    private double _fighterSpeed = 100;
    private double _droneSpeed = 60;
    private double _interceptorSpeed = 200;
    private int _maxDefenseMissiles = 10;
    private double _threatDetectionDelay = 0.5;
    private double _cityDamageThreshold = 0.7;
    private bool _autoEvacuation = true;
    private string _selectedChart = "Bar";
    private int _defenseRocketCapacity = 20;
    private int _peopleInBunkers;
    private int _buildingsDestroyed;
    private double _cityHealthPercentage = 100;
    private int _threatsMissed;
    private DateTime _simulationStartTime;
    private bool _simulationEnded = false;
    private TimelineTracker _timeline = new TimelineTracker();
    public int BuildingsDestroyed
    {
        get => _buildingsDestroyed;
        set { _buildingsDestroyed = value; OnPropertyChanged(); }
    }

    private string _ElapsedTimeFormatted = "00:00:00";
    public string ElapsedTimeFormatted
    {
        get => _ElapsedTimeFormatted;
        set 
        { 
            _ElapsedTimeFormatted = value; OnPropertyChanged(); 
        }
    }
    public double CityHealthPercentage
    {
        get => _cityHealthPercentage;
        set { _cityHealthPercentage = value; OnPropertyChanged(); }
    }

    public int ThreatsMissed
    {
        get => _threatsMissed;
        set { _threatsMissed = value; OnPropertyChanged(); }
    }
    public bool IsRunning
    {
        get => _isRunning;
        set { _isRunning = value; OnPropertyChanged(); }
    }

    public double SimulationSpeed
    {
        get => _simulationSpeed;
        set
        {
            var validated = ValidateRange(value, 0.1, 3.0);
            if (Math.Abs(_simulationSpeed - validated) > 0.001)
            {
                _simulationSpeed = validated;
                OnPropertyChanged(nameof(SimulationSpeed));
            }
        }
    }

    public int EnemyFighterCount
    {
        get => _enemyFighterCount;
        set
        {
            var validated = ValidateRange(value, 0, 15);
            if (_enemyFighterCount != validated)
            {
                _enemyFighterCount = validated;
                OnPropertyChanged(nameof(EnemyFighterCount));
            }
        }
    }

    public int EnemyMissileCount
    {
        get => _enemyMissileCount;
        set
        {
            var validated = ValidateRange(value, 0, 20);
            if (_enemyMissileCount != validated)
            {
                _enemyMissileCount = validated;
                OnPropertyChanged(nameof(EnemyMissileCount));
            }
        }
    }

    public int EnemyDroneCount
    {
        get => _enemyDroneCount;
        set
        {
            var validated = ValidateRange(value, 0, 15);
            if (_enemyDroneCount != validated)
            {
                _enemyDroneCount = validated;
                OnPropertyChanged(nameof(EnemyDroneCount));
            }
        }
    }

    public double DetectionRadius
    {
        get => _detectionRadius;
        set
        {
            var validated = ValidateRange(value, 100, 280);
            if (Math.Abs(_detectionRadius - validated) > 0.001)
            {
                _detectionRadius = validated;
                OnPropertyChanged(nameof(DetectionRadius));
            }
        }
    }

    // Property mit Validierung: InterceptionRadius (50-350)
    public double InterceptionRadius
    {
        get => _interceptionRadius;
        set
        {
            var validated = ValidateRange(value, 50, 280);
            if (Math.Abs(_interceptionRadius - validated) > 0.001)
            {
                _interceptionRadius = validated;
                OnPropertyChanged(nameof(InterceptionRadius));
            }
        }
    }

    public int PopulationCount
    {
        get => _populationCount;
        set
        {
            var validated = ValidateRange(value, 10, 300);
            if (_populationCount != validated)
            {
                _populationCount = validated;
                OnPropertyChanged(nameof(PopulationCount));
            }
        }
    }

    public int TotalSpawned
    {
        get => _totalSpawned;
        set { _totalSpawned = value; OnPropertyChanged(); }
    }

    public int ActiveObjects
    {
        get => _activeObjects;
        set { _activeObjects = value; OnPropertyChanged(); }
    }

    public int DestroyedObjects
    {
        get => _destroyedObjects;
        set { _destroyedObjects = value; OnPropertyChanged(); }
    }

    public int MissilesFired
    {
        get => _missilesFired;
        set { _missilesFired = value; OnPropertyChanged(); }
    }

    public int SuccessfulInterceptions
    {
        get => _successfulInterceptions;
        set { _successfulInterceptions = value; OnPropertyChanged(); }
    }

    public double ElapsedTime
    {
        get => _elapsedTime;
        set { _elapsedTime = value; OnPropertyChanged(); }
    }

    public int EvacuatedCount
    {
        get => _evacuatedCount;
        set { _evacuatedCount = value; OnPropertyChanged(); }
    }

    public int CasualtiesCount
    {
        get => _casualtiesCount;
        set { _casualtiesCount = value; OnPropertyChanged(); }
    }

    public double MissileSpeed
    {
        get => _missileSpeed;
        set { _missileSpeed = value; OnPropertyChanged(); }
    }

    public double FighterSpeed
    {
        get => _fighterSpeed;
        set { _fighterSpeed = value; OnPropertyChanged(); }
    }

    public double DroneSpeed
    {
        get => _droneSpeed;
        set { _droneSpeed = value; OnPropertyChanged(); }
    }

    public double InterceptorSpeed
    {
        get => _interceptorSpeed;
        set { _interceptorSpeed = value; OnPropertyChanged(); }
    }

    public int MaxDefenseMissiles
    {
        get => _maxDefenseMissiles;
        set { _maxDefenseMissiles = value; OnPropertyChanged(); }
    }

    public double ThreatDetectionDelay
    {
        get => _threatDetectionDelay;
        set { _threatDetectionDelay = value; OnPropertyChanged(); }
    }

    public double CityDamageThreshold
    {
        get => _cityDamageThreshold;
        set { _cityDamageThreshold = value; OnPropertyChanged(); }
    }

    public bool AutoEvacuation
    {
        get => _autoEvacuation;
        set { _autoEvacuation = value; OnPropertyChanged(); }
    }

    public string SelectedChart
    {
        get => _selectedChart;
        set { _selectedChart = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public int DefenseRocketCapacity
    {
        get => _defenseRocketCapacity;
        set
        {
            var validated = ValidateRange(value, 5, 50);
            if (_defenseRocketCapacity != validated)
            {
                _defenseRocketCapacity = validated;
                OnPropertyChanged(nameof(DefenseRocketCapacity));
            }
        }
    }
    // Validierungsmethode für int-Werte
    private int ValidateRange(int value, int min, int max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    // Validierungsmethode für double-Werte
    private double ValidateRange(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }


    public int PeopleInBunkers
    {
        get => _peopleInBunkers;
        set { _peopleInBunkers = value; OnPropertyChanged(); }
    }
}