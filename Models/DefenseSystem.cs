using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Media;

namespace AirDefenseSimulation.Models;

public class DefenseSystem
{
    public Vector2D Position { get; set; }
    public double DetectionRadius { get; set; }
    public double InterceptionRadius { get; set; }
    public List<AirObject> DetectedThreats { get; set; }
    public List<DefenseMissile> ActiveMissiles { get; set; }
    public double RadarAngle { get; set; }
    public int MissilesLaunched { get; set; }
    public int SuccessfulInterceptions { get; set; }

    public DefenseSystem(Vector2D position, double detectionRadius, double interceptionRadius)
    {
        Position = position;
        DetectionRadius = detectionRadius;
        InterceptionRadius = interceptionRadius;
        DetectedThreats = new List<AirObject>();
        ActiveMissiles = new List<DefenseMissile>();
        RadarAngle = 0;
    }

    public void Update(double deltaTime, List<AirObject> threats, City? city = null)
    {
        RadarAngle += deltaTime * 2;

        DetectedThreats.Clear();
        foreach (var threat in threats)
        {
            if (!threat.IsActive || threat.IsDestroyed) continue;

            var distance = Vector2D.Distance(Position, threat.Position);
            if (distance <= DetectionRadius)
            {
                DetectedThreats.Add(threat);

                // Эвакуация при обнаружении угрозы близко к городу
                if (city != null && distance < InterceptionRadius * 1.5)
                {
                    city.TriggerEvacuation(threat.Position, 80);
                }

                if (distance <= InterceptionRadius && !ActiveMissiles.Any(m => m.TargetObject == threat && m.IsActive))
                {
                    LaunchInterceptor(threat);
                }
            }
        }

        ActiveMissiles.RemoveAll(m => !m.IsActive);
        foreach (var missile in ActiveMissiles)
        {
            var wasActive = missile.IsActive;
            missile.Update(deltaTime);

            if (wasActive && !missile.IsActive && missile.TargetObject != null && missile.TargetObject.IsDestroyed)
            {
                SuccessfulInterceptions++;

                // Создание взрыва при успешном перехвате
                if (city != null)
                {
                    city.Explosions.Add(new Explosion(missile.Position, 20, true));
                }
            }
        }

        // Проверка попаданий по городу
        if (city != null)
        {
            foreach (var threat in threats.Where(t => !t.IsActive && !t.IsDestroyed))
            {
                var distanceToCity = Vector2D.Distance(threat.Position, city.Center);
                if (distanceToCity < city.Radius)
                {
                    // Регистрация удара
                    double impactRadius = threat.Type switch
                    {
                        AirObjectType.EnemyMissile => 35,
                        AirObjectType.EnemyFighter => 25,
                        AirObjectType.EnemyDrone => 20,
                        _ => 15
                    };

                    city.RegisterImpact(threat.Position, impactRadius, false);
                    threat.IsDestroyed = true;
                }
            }
        }
    }

    private void LaunchInterceptor(AirObject target)
    {
        var missile = new DefenseMissile(
            $"defense-{MissilesLaunched++}",
            Position,
            target,
            200);

        ActiveMissiles.Add(missile);
    }

    public void Draw(DrawingContext context)
    {
        var detectionPen = new Pen(new SolidColorBrush(Color.FromArgb(80, 52, 211, 153)), 2)
        {
            DashStyle = new DashStyle(new double[] { 5, 5 }, 0)
        };
        context.DrawEllipse(null, detectionPen, new Point(Position.X, Position.Y),
            DetectionRadius, DetectionRadius);

        var interceptionPen = new Pen(new SolidColorBrush(Color.FromArgb(120, 52, 211, 153)), 2);
        context.DrawEllipse(null, interceptionPen, new Point(Position.X, Position.Y),
            InterceptionRadius, InterceptionRadius);

        var stationBrush = new SolidColorBrush(Color.FromRgb(52, 211, 153));
        context.DrawRectangle(stationBrush, new Pen(Brushes.Black, 2),
            new Rect(Position.X - 12, Position.Y - 12, 24, 24));

        context.DrawLine(new Pen(stationBrush, 3),
            new Point(Position.X, Position.Y - 12),
            new Point(Position.X, Position.Y - 25));

        var radarX = Position.X + Math.Cos(RadarAngle) * DetectionRadius;
        var radarY = Position.Y + Math.Sin(RadarAngle) * DetectionRadius;
        var radarPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 52, 211, 153)), 2);
        context.DrawLine(radarPen, new Point(Position.X, Position.Y), new Point(radarX, radarY));

        foreach (var missile in ActiveMissiles)
        {
            missile.Draw(context);
        }

        var targetPen = new Pen(new SolidColorBrush(Color.FromArgb(60, 255, 50, 50)), 1)
        {
            DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
        };
        foreach (var threat in DetectedThreats)
        {
            context.DrawLine(targetPen,
                new Point(Position.X, Position.Y),
                new Point(threat.Position.X, threat.Position.Y));
        }
    }
}
