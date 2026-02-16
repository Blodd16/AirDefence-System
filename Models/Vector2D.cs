using System;

namespace AirDefenseSimulation.Models;

public struct Vector2D
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    public double Length => Math.Sqrt(X * X + Y * Y);

    public Vector2D Normalized
    {
        get
        {
            var len = Length;
            return len > 0 ? new Vector2D(X / len, Y / len) : this;
        }
    }

    public static Vector2D operator +(Vector2D a, Vector2D b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2D operator -(Vector2D a, Vector2D b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2D operator *(Vector2D a, double scalar) => new(a.X * scalar, a.Y * scalar);
    public static double Distance(Vector2D a, Vector2D b) => (b - a).Length;
}
