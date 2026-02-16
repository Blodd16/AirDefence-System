using System;
using Avalonia;
using Avalonia.Media;

namespace AirDefenseSimulation.Models;

public class EnemyDrone : AirObject
{
    private double rotorAngle;

    public EnemyDrone(string id, Vector2D position, Vector2D target, double speed)
        : base(id, position, target, speed, AirObjectType.EnemyDrone)
    {
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);
        rotorAngle += deltaTime * 20;
    }

    public override void Draw(DrawingContext context)
    {
        DrawTrail(context, Colors.Purple);

        if (!IsActive && !IsDestroyed)
            return;

        if (IsDestroyed)
        {
            var explosionBrush = new SolidColorBrush(Color.FromArgb(180, 200, 100, 255));
            context.DrawEllipse(explosionBrush, null, new Point(Position.X, Position.Y), 10, 10);
            return;
        }

        var droneBrush = new SolidColorBrush(Colors.Purple);
        context.DrawRectangle(droneBrush, null, new Rect(Position.X - 6, Position.Y - 6, 12, 12));

        var rotorPositions = new[]
        {
        new Point(-8, -8), new Point(8, -8),
        new Point(8, 8),   new Point(-8, 8)
    };

        foreach (var rotorPos in rotorPositions)
        {
            var rotorCenter = new Point(Position.X + rotorPos.X, Position.Y + rotorPos.Y);
            using (context.PushTransform(Matrix.CreateTranslation(rotorCenter.X, rotorCenter.Y)))
            using (context.PushTransform(Matrix.CreateRotation(rotorAngle)))
            {
                var rotorPen = new Pen(new SolidColorBrush(Color.FromArgb(150, 139, 92, 246)), 2);

                context.DrawLine(rotorPen, new Point(-5, 0), new Point(5, 0));
                context.DrawLine(rotorPen, new Point(0, -5), new Point(0, 5));
            }
            context.DrawLine(
                new Pen(droneBrush, 1),
                new Point(Position.X, Position.Y),
                new Point(
                    rotorCenter.X * 0.7 + Position.X * 0.3,
                    rotorCenter.Y * 0.7 + Position.Y * 0.3
                ));
        }
    }

}
