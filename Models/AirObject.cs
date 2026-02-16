using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;

namespace AirDefenseSimulation.Models;

public enum AirObjectType
{
    EnemyFighter,
    EnemyMissile,
    EnemyDrone,
    DefenseMissile
}

public abstract class AirObject
{
    public string Id { get; set; }
    public Vector2D Position { get; set; }
    public Vector2D Velocity { get; set; }
    public Vector2D Target { get; set; }
    public double Speed { get; set; }
    public bool IsActive { get; set; }
    public bool IsDestroyed { get; set; }
    public AirObjectType Type { get; set; }
    public List<Vector2D> Trail { get; set; }
    public double Angle { get; set; }

    protected AirObject(string id, Vector2D position, Vector2D target, double speed, AirObjectType type)
    {
        Id = id;
        Position = position;
        Target = target;
        Speed = speed;
        Type = type;
        IsActive = true;
        IsDestroyed = false;
        Trail = new List<Vector2D>();
    }

    public virtual void Update(double deltaTime)
    {
        if (!IsActive || IsDestroyed) return;

        var direction = Target - Position;
        var distance = direction.Length;

        if (distance < Speed * deltaTime)
        {
            Position = Target;
            IsActive = false;
            return;
        }

        Angle = Math.Atan2(direction.Y, direction.X);
        Velocity = direction.Normalized * Speed;
        Position += Velocity * deltaTime;

        Trail.Add(Position);
        if (Trail.Count > 30)
            Trail.RemoveAt(0);
    }

    public abstract void Draw(DrawingContext context);

    protected void DrawTrail(DrawingContext context, Color color)
    {
        if (Trail.Count < 2) return;

        var pen = new Pen(new SolidColorBrush(Color.FromArgb(80, color.R, color.G, color.B)), 2);
        for (int i = 0; i < Trail.Count - 1; i++)
        {
            context.DrawLine(pen,
                new Point(Trail[i].X, Trail[i].Y),
                new Point(Trail[i + 1].X, Trail[i + 1].Y));
        }
    }
}
