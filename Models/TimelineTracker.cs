using System;
using System.Collections.Generic;

namespace AirDefenseSimulation.Models;

// Типы событий в симуляции
public enum EventType
{
    ThreatSpawned,      // Угроза появилась 
    MissileLaunched,    // Ракета обороны запущена
    Interception,       // Успешный перехват
    CityHit,           // Попадание в город
    BuildingDestroyed, // Здание уничтожено
    PeopleEvacuated,   // Люди эвакуированы
    Casualties         // Жертвы
}

// Класс для хранения одного события
public class TimelineEvent
{
    public double Time { get; set; }              // Время в секундах
    public EventType Type { get; set; }           // Тип события
    public string Description { get; set; } = ""; // Описание
    public int Value { get; set; }                // Численное значение (кол-во жертв, перехватов и т.д.)
    public string Location { get; set; } = "";    // Локация события (если нужно)
}

// Класс для снимка статистики в определённый момент времени
public class TimelineSnapshot
{
    public double Time { get; set; }
    public int ActiveThreats { get; set; }
    public int TotalInterceptions { get; set; }
    public int TotalCasualties { get; set; }
    public double CityHealth { get; set; }
    public int PeopleInBunkers { get; set; }
}

// Основной класс для отслеживания временной шкалы
public class TimelineTracker
{
    public List<TimelineEvent> Events { get; } = new();
    public List<TimelineSnapshot> Snapshots { get; } = new();

    private double _lastSnapshotTime = -1.0;
    private const double SnapshotInterval = 1.0; // Снимок каждую секунду

    // Добавление события
    public void AddEvent(double time, EventType type, string description, int value = 0)
    {
        Events.Add(new TimelineEvent
        {
            Time = time,
            Type = type,
            Description = description,
            Value = value
        });
    }

    // Создание снимка текущего состояния
    public void TakeSnapshot(double time, int activeThreats, int totalInterceptions,
                            int totalCasualties, double cityHealth, int peopleInBunkers)
    {
        if (Snapshots.Count == 0 || time - _lastSnapshotTime >= SnapshotInterval)
        {
            Snapshots.Add(new TimelineSnapshot
            {
                Time = time,
                ActiveThreats = activeThreats,
                TotalInterceptions = totalInterceptions,
                TotalCasualties = totalCasualties,
                CityHealth = cityHealth,
                PeopleInBunkers = peopleInBunkers
            });

            _lastSnapshotTime = time;
        }
    }

    // Получить события в определённом временном диапазоне
    public List<TimelineEvent> GetEventsInRange(double startTime, double endTime)
    {
        return Events.FindAll(e => e.Time >= startTime && e.Time <= endTime);
    }

    // Получить ключевые моменты (когда произошли важные изменения)
    public List<TimelineEvent> GetKeyMoments()
    {
        return Events.FindAll(e =>
            e.Type == EventType.CityHit ||
            e.Type == EventType.BuildingDestroyed ||
            (e.Type == EventType.Casualties && e.Value >= 5));
    }

    // Очистка данных
    public void Clear()

    {
        Events.Clear();
        Snapshots.Clear();
        _lastSnapshotTime = -1.0;
    }

    // Форматирование времени в читаемый вид
    public static string FormatTime(double seconds)
    {
        var minutes = (int)(seconds / 60);
        var secs = (int)(seconds % 60);
        return $"{minutes}:{secs:D2}";
    }
}