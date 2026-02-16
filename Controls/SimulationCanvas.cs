using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using AirDefenseSimulation.Models;
using System.Collections.Generic;

namespace AirDefenseSimulation.Controls;

public class SimulationCanvas : Control
{
    public City? City { get; set; }
    public DefenseSystem? DefenseSystem { get; set; }
    public List<AirObject> Enemies { get; set; } = new();

    public override void Render(DrawingContext context) 
    {
        base.Render(context);

        context.DrawRectangle(new SolidColorBrush(Color.FromRgb(15, 23, 42)), null,
            new Rect(0, 0, Bounds.Width, Bounds.Height));
        
        DrawGrid(context);

        City?.Draw(context);
        DefenseSystem?.Draw(context);

        foreach (var enemy in Enemies)
        {
            enemy.Draw(context);
        }
    }

    private void DrawGrid(DrawingContext context)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(30, 100, 100, 100)), 1);

        for (double x = 0; x < Bounds.Width; x += 50)
        {
            context.DrawLine(gridPen, new Point(x, 0), new Point(x, Bounds.Height));
        }

        for (double y = 0; y < Bounds.Height; y += 50)
        {
            context.DrawLine(gridPen, new Point(0, y), new Point(Bounds.Width, y));
        }
    }
}
