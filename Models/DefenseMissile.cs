using System;
using Avalonia;
using Avalonia.Media;

namespace AirDefenseSimulation.Models;

public class DefenseMissile : AirObject
{
    public AirObject? TargetObject { get; set; }
    private const double InterceptDistance = 15;

    public DefenseMissile(string id, Vector2D position, AirObject target, double speed)
        : base(id, position, target.Position, speed, AirObjectType.DefenseMissile)
    {
        TargetObject = target;
    }

    public override void Update(double deltaTime)
    {
        if (!IsActive || IsDestroyed) return;
        if (TargetObject != null && TargetObject.IsActive && !TargetObject.IsDestroyed)
        {
            Target = TargetObject.Position;

            var distanceToTarget = Vector2D.Distance(Position, TargetObject.Position);
            if (distanceToTarget < InterceptDistance)
            {
                IsActive = false;
                TargetObject.IsDestroyed = true;
                TargetObject.IsActive = false;
                return;
            }
        }
        else
        {
            IsActive = false;
            return;
        }

        var direction = Target - Position;
        var distance = direction.Length;

        if (distance < Speed * deltaTime)
        {
            Position = Target;
        }
        else
        {
            Angle = Math.Atan2(direction.Y, direction.X);
            Velocity = direction.Normalized * Speed;
            Position += Velocity * deltaTime;
        }

        Trail.Add(Position);
        if (Trail.Count > 20)
            Trail.RemoveAt(0);
    }

    public override void Draw(DrawingContext context)
    {
        DrawTrail(context, Colors.Cyan);

        if (!IsActive)
            return;

        using (context.PushTransform(Matrix.CreateTranslation(Position.X, Position.Y)))
        using (context.PushTransform(Matrix.CreateRotation(Angle)))
        {
            var defenseBrush = new SolidColorBrush(Colors.Cyan);

            var bodyGeometry = new PathGeometry
            {
                Figures = new PathFigures
            {
                new PathFigure
                {
                    StartPoint = new Point(8, 0),
                    Segments = new PathSegments
                    {
                        new LineSegment { Point = new Point(-6, -2) },
                        new LineSegment { Point = new Point(-6, 2) }
                    },
                    IsClosed = true
                }
            }
            };

            context.DrawGeometry(defenseBrush, null, bodyGeometry);

            var flameBrush = new SolidColorBrush(Color.FromArgb(200, 0, 255, 255));
            context.DrawEllipse(flameBrush, null, new Point(-6, 0), 3, 3);
        } 
    }

}
