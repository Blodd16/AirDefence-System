using System;
using Avalonia;
using Avalonia.Media;

namespace AirDefenseSimulation.Models;

public class EnemyMissile : AirObject
{
    public EnemyMissile(string id, Vector2D position, Vector2D target, double speed)
        : base(id, position, target, speed, AirObjectType.EnemyMissile)
    {
    }

    public override void Draw(DrawingContext context)
    {
        DrawTrail(context, Colors.Orange);

        if (!IsActive && !IsDestroyed)
            return;

        if (IsDestroyed)
        {
            var explosionBrush = new SolidColorBrush(Color.FromArgb(180, 255, 80, 0));
            context.DrawEllipse(explosionBrush, null, new Point(Position.X, Position.Y), 12, 12);
            return;
        }

        using (context.PushTransform(Matrix.CreateTranslation(Position.X, Position.Y)))
        using (context.PushTransform(Matrix.CreateRotation(Angle)))
        {
            var missileBrush = new SolidColorBrush(Colors.Orange);

            var bodyGeometry = new PathGeometry
            {
                Figures = new PathFigures
            {
                new PathFigure
                {
                    StartPoint = new Point(10, 0),
                    Segments = new PathSegments
                    {
                        new LineSegment { Point = new Point(-8, -3) },
                        new LineSegment { Point = new Point(-8, 3) }
                    },
                    IsClosed = true
                }
            }
            };
            context.DrawGeometry(missileBrush, null, bodyGeometry);

            context.DrawLine(new Pen(missileBrush, 2), new Point(-4, -3), new Point(-6, -6));
            context.DrawLine(new Pen(missileBrush, 2), new Point(-4, 3), new Point(-6, 6));

            var flameBrush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops = new GradientStops
            {
                new GradientStop { Color = Color.FromArgb(255, 255, 107, 0), Offset = 0 },
                new GradientStop { Color = Color.FromArgb(0, 255, 149, 0), Offset = 1 }
            }
            };

            var flameGeometry = new PathGeometry
            {
                Figures = new PathFigures
            {
                new PathFigure
                {
                    StartPoint = new Point(-8, -2),
                    Segments = new PathSegments
                    {
                        new LineSegment { Point = new Point(-16, 0) },
                        new LineSegment { Point = new Point(-8, 2) }
                    },
                    IsClosed = true
                }
            }
            };

            context.DrawGeometry(flameBrush, null, flameGeometry);
        } 
    }

}
