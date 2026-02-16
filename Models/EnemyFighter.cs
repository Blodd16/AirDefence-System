using System;
using Avalonia;
using Avalonia.Media;

namespace AirDefenseSimulation.Models;

public class EnemyFighter : AirObject
{
    public EnemyFighter(string id, Vector2D position, Vector2D target, double speed)
        : base(id, position, target, speed, AirObjectType.EnemyFighter)
    {
    }

    public override void Draw(DrawingContext context)
    {
        DrawTrail(context, Colors.Red);

        if (!IsActive && !IsDestroyed)
            return;

        if (IsDestroyed)
        {
            var explosionBrush = new SolidColorBrush(Color.FromArgb(180, 255, 100, 0));
            context.DrawEllipse(explosionBrush, null, new Point(Position.X, Position.Y), 15, 15);
            return;
        }

        using (context.PushTransform(Matrix.CreateTranslation(Position.X, Position.Y)))
        using (context.PushTransform(Matrix.CreateRotation(Angle)))
        {
            var bodyBrush = new SolidColorBrush(Colors.Red);

            var bodyGeometry = new PathGeometry
            {
                Figures = new PathFigures
            {
                new PathFigure
                {
                    StartPoint = new Point(12, 0),
                    Segments = new PathSegments
                    {
                        new LineSegment { Point = new Point(-8, -6) },
                        new LineSegment { Point = new Point(-6, 0) },
                        new LineSegment { Point = new Point(-8, 6) }
                    },
                    IsClosed = true
                }
            }
            };

            context.DrawGeometry(bodyBrush, null, bodyGeometry);

            context.DrawLine(new Pen(bodyBrush, 3), new Point(-2, -6), new Point(-6, -12));
            context.DrawLine(new Pen(bodyBrush, 3), new Point(-2, 6), new Point(-6, 12));

            var engineBrush = new SolidColorBrush(Color.FromArgb(200, 255, 150, 0));
            context.DrawEllipse(engineBrush, null, new Point(-8, 0), 3, 3);
        }
    }
}
