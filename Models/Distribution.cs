using System;

namespace AirDefenseSimulation.Models;

public enum DistributionType
{
    Normal,
    Uniform,
    Exponential
}

public static class Distribution
{
    private static readonly Random Random = new();

    public static double Normal(double mean, double stdDev)
    {
        var u1 = Random.NextDouble();
        var u2 = Random.NextDouble();
        var z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * z0;
    }

    public static double Uniform(double min, double max)
    {
        return min + Random.NextDouble() * (max - min);
    }

    public static double Exponential(double lambda)
    {
        return -Math.Log(1 - Random.NextDouble()) / lambda;
    }

    public static double Generate(DistributionType type, double param1, double param2)
    {
        return type switch
        {
            DistributionType.Normal => Normal(param1, param2),
            DistributionType.Uniform => Uniform(param1, param2),
            DistributionType.Exponential => Exponential(param1),
            _ => param1
        };
    }
}
