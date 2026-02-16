using Avalonia;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AirDefenseSimulation.Models;

public class Building
{
    public Rect Bounds { get; set; }
    public int Height { get; set; }
    public Color Color { get; set; }
    public bool IsTower { get; set; }
    public bool IsDestroyed { get; set; }
    public double Damage { get; set; }
    public bool IsOnFire { get; set; }
    public double FireIntensity { get; set; }
}

public class Bunker
{
    public Vector2D Position { get; set; }
    public int Capacity { get; set; }
    public int CurrentOccupants { get; set; }
    public Rect Bounds { get; set; }
    public bool IsOverloaded => CurrentOccupants > Capacity;

    public Bunker(Vector2D position, int capacity)
    {
        Position = position;
        Capacity = capacity;
        CurrentOccupants = 0;
        Bounds = new Rect(position.X - 15, position.Y - 15, 30, 30);
    }

    public bool HasSpace => CurrentOccupants < Capacity;
}

public class Person
{
    public Vector2D Position { get; set; }
    public Vector2D Velocity { get; set; }
    public bool IsEvacuating { get; set; }
    public Vector2D EvacuationTarget { get; set; }
    public Color Color { get; set; }
    public double PanicLevel { get; set; }

    public Person(Vector2D position)
    {
        Position = position;
        Velocity = new Vector2D(0, 0);
        IsEvacuating = false;
        PanicLevel = 0;
        var random = new Random();
        Color = Color.FromRgb(
            (byte)random.Next(100, 255),
            (byte)random.Next(100, 255),
            (byte)random.Next(100, 255));
    }

    public void Update(double deltaTime)
    {
        if (!IsEvacuating) return;

        var direction = EvacuationTarget - Position;
        var distance = direction.Length;

        if (distance < 5)
        {
            IsEvacuating = false;
            return;
        }

        var speed = 50 + (PanicLevel * 30);
        Velocity = direction.Normalized * speed;
        Position += Velocity * deltaTime;

        PanicLevel = Math.Max(0, PanicLevel - deltaTime * 0.5);
    }
}

public class Vehicle
{
    public Vector2D Position { get; set; }
    public Vector2D Velocity { get; set; }
    public Vector2D Target { get; set; }
    public Color Color { get; set; }
    public bool IsActive { get; set; }
    public bool IsAmbulance { get; set; }

    public Vehicle(Vector2D position, Vector2D target, bool isAmbulance = false)
    {
        Position = position;
        Target = target;
        IsActive = true;
        IsAmbulance = isAmbulance;
        var random = new Random();
        Color = isAmbulance ? Colors.White : Color.FromRgb(
            (byte)random.Next(150, 255),
            (byte)random.Next(150, 255),
            (byte)random.Next(50, 100));
    }

    public void Update(double deltaTime)
    {
        if (!IsActive) return;

        var direction = Target - Position;
        var distance = direction.Length;

        if (distance < 3)
        {
            var random = new Random();
            Target = new Vector2D(
                Position.X + random.Next(-50, 50),
                Position.Y + random.Next(-50, 50));
        }

        Velocity = direction.Normalized * 30;
        Position += Velocity * deltaTime;
    }
}

public class Explosion
{
    public Vector2D Position { get; set; }
    public double Size { get; set; }
    public double Life { get; set; }
    public double MaxLife { get; set; }
    public int CasualtiesCount { get; set; }
    public bool WasIntercepted { get; set; }

    public Explosion(Vector2D position, double size = 20, bool intercepted = false)
    {
        Position = position;
        Size = size;
        Life = 1.0;
        MaxLife = 1.0;
        CasualtiesCount = 0;
        WasIntercepted = intercepted;
    }

    public void Update(double deltaTime)
    {
        Life -= deltaTime * 2;
        Size += deltaTime * 15;
    }

    public bool IsAlive => Life > 0;
}

public class City
{
    public Vector2D Center { get; set; }
    public double Radius { get; set; }
    public List<Building> Buildings { get; set; }
    public List<Person> People { get; set; }
    public List<Vehicle> Vehicles { get; set; }
    public List<Explosion> Explosions { get; set; }
    public List<Bunker> Bunkers { get; set; }

    public int InitialPopulation { get; set; }
    public int EvacuatedCount { get; set; }
    public int CasualtiesCount { get; set; }
    public int BunkerCount { get; set; }
    public int PeopleInBunkers { get; set; }
    public int BuildingsDestroyed { get; set; }
    public double CityHealthPercentage { get; set; }
    public int ThreatsMissed { get; set; }

    public City(Vector2D center, double radius, int population)
    {
        Center = center;
        Radius = radius;
        InitialPopulation = population;
        Buildings = new List<Building>();
        People = new List<Person>();
        Vehicles = new List<Vehicle>();
        Explosions = new List<Explosion>();
        Bunkers = new List<Bunker>();
        EvacuatedCount = 0;
        CasualtiesCount = 0;
        PeopleInBunkers = 0;
        BuildingsDestroyed = 0;
        CityHealthPercentage = 100;
        ThreatsMissed = 0;

        BunkerCount = Math.Max(3, population / 30);

        GenerateBuildings();
        GenerateBunkers();
        GeneratePeople(population);
        GenerateVehicles();
    }

    private void GenerateBuildings()
    {
        var random = new Random();

        for (int i = 0; i < 30; i++)
        {
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * (Radius * 0.6);

            var x = Center.X + Math.Cos(angle) * distance;
            var y = Center.Y + Math.Sin(angle) * distance;

            var width = random.Next(20, 40);
            var height = random.Next(40, 70);

            Buildings.Add(new Building
            {
                Bounds = new Rect(x - width / 2, y - height, width, height),
                Height = height,
                IsTower = false,
                IsDestroyed = false,
                Damage = 0,
                IsOnFire = false,
                FireIntensity = 0,
                Color = Color.FromRgb(
                    (byte)random.Next(80, 120),
                    (byte)random.Next(80, 120),
                    (byte)random.Next(80, 120))
            });
        }

        for (int i = 0; i < 3; i++)
        {
            var angle = (i / 3.0) * 2 * Math.PI;
            double towerWidth = 30;
            double towerHeight = 80; //make taller
            double maxDistance = Radius - towerHeight; // the top of the tower should be within the city radius
            double distance = maxDistance * 0.8; // a bit inside the max distance

            var x = Center.X + Math.Cos(angle) * distance;
            var y = Center.Y + Math.Sin(angle) * distance;

            Buildings.Add(new Building
            {
                Bounds = new Rect(x - towerWidth / 2, y - towerHeight, towerWidth, towerHeight),
                
                IsTower = true,
                IsDestroyed = false,
                Damage = 0,
                IsOnFire = false,
                FireIntensity = 0,
                Color = Color.FromRgb(100, 100, 120)
            });
        }

    }

    private void GenerateBunkers()
    {
        var bunkerCapacity = InitialPopulation / BunkerCount;

        for (int i = 0; i < BunkerCount; i++)
        {
            var angle = (i / (double)BunkerCount) * 2 * Math.PI;
            var distance = Radius * 0.6;

            var x = Center.X + Math.Cos(angle) * distance;
            var y = Center.Y + Math.Sin(angle) * distance;

            Bunkers.Add(new Bunker(new Vector2D(x, y), bunkerCapacity + 10));
        }
    }

    private void GeneratePeople(int count)
    {
        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * (Radius * 0.85);

            var x = Center.X + Math.Cos(angle) * distance;
            var y = Center.Y + Math.Sin(angle) * distance;

            People.Add(new Person(new Vector2D(x, y)));
        }
    }

    private void GenerateVehicles()
    {
        var random = new Random();

        // Обычные машины
        for (int i = 0; i < 4; i++)
        {
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * (Radius * 0.8);

            var x = Center.X + Math.Cos(angle) * distance;
            var y = Center.Y + Math.Sin(angle) * distance;

            var targetAngle = random.NextDouble() * 2 * Math.PI;
            var targetX = Center.X + Math.Cos(targetAngle) * random.NextDouble() * Radius;
            var targetY = Center.Y + Math.Sin(targetAngle) * random.NextDouble() * Radius;

            Vehicles.Add(new Vehicle(
                new Vector2D(x, y),
                new Vector2D(targetX, targetY)));
        }

        // Ambulances 
        for (int i = 0; i < 2; i++)
        {
            var angle = random.NextDouble() * 2 * Math.PI;
            var distance = random.NextDouble() * (Radius * 0.7);

            var x = Center.X + Math.Cos(angle) * distance;
            var y = Center.Y + Math.Sin(angle) * distance;

        }
    }

    public void Update(double deltaTime)
    {
        foreach (var person in People.ToList())
        {
            person.Update(deltaTime);
        }

        foreach (var vehicle in Vehicles)
        {
            vehicle.Update(deltaTime);
        }

        Explosions.RemoveAll(e => !e.IsAlive);
        foreach (var explosion in Explosions)
        {
            explosion.Update(deltaTime);
        }

        // Обновление зданий в огне
        foreach (var building in Buildings.Where(b => b.IsOnFire && !b.IsDestroyed))
        {
            building.FireIntensity += deltaTime * 0.3;
            building.Damage += deltaTime * 0.1;

            if (building.Damage >= 1.0)
            {
                building.IsDestroyed = true;
                building.IsOnFire = false;
                BuildingsDestroyed++;
            }
        }

        // Проверка достижения бункеров
        foreach (var bunker in Bunkers)
        {
            var arrivedPeople = People.Where(p =>
                p.IsEvacuating &&
                Vector2D.Distance(p.Position, bunker.Position) < 20).ToList();

            foreach (var person in arrivedPeople)
            {
                if (bunker.HasSpace)
                {
                    bunker.CurrentOccupants++;
                    PeopleInBunkers++;
                    person.IsEvacuating = false;
                    People.Remove(person);
                }
            }
        }



        // Count health of the city
        var totalBuildings = Buildings.Count;
        var destroyedBuildings = Buildings.Count(b => b.IsDestroyed);
        CityHealthPercentage = ((totalBuildings - destroyedBuildings) / (double)totalBuildings) * 100;
    }

    public void TriggerEvacuation(Vector2D threatPosition, double threatRadius)
    {
        foreach (var person in People)
        {
            var distance = Vector2D.Distance(person.Position, threatPosition);
            if (distance < threatRadius && !person.IsEvacuating)
            {
                person.IsEvacuating = true;
                person.PanicLevel = Math.Min(1.0, (threatRadius - distance) / threatRadius);

                var nearestBunker = Bunkers
                    .Where(b => b.HasSpace)
                    .OrderBy(b => Vector2D.Distance(person.Position, b.Position))
                    .FirstOrDefault();

                if (nearestBunker != null)
                {
                    person.EvacuationTarget = nearestBunker.Position;
                }
                
                
                  
              
            }
        }
    }

    public void RegisterImpact(Vector2D impactPosition, double impactRadius, bool wasIntercepted = false)
    {
        ThreatsMissed++;

        // Жертвы среди людей
        var casualties = People.Where(p =>
            Vector2D.Distance(p.Position, impactPosition) < impactRadius).ToList();

        CasualtiesCount += casualties.Count;

        var explosion = new Explosion(impactPosition, wasIntercepted ? 20 : 30, wasIntercepted)
        {
            CasualtiesCount = casualties.Count
        };
        Explosions.Add(explosion);

        foreach (var casualty in casualties)
        {
            People.Remove(casualty);
        }

        // Повреждение зданий
        foreach (var building in Buildings)
        {
            var buildingCenter = new Vector2D(
                building.Bounds.Center.X,
                building.Bounds.Center.Y);

            var distance = Vector2D.Distance(buildingCenter, impactPosition);

            if (distance < impactRadius * 2)
            {
                building.Damage += 0.4;

                if (!building.IsOnFire && distance < impactRadius * 1.5)
                {
                    building.IsOnFire = true;
                    building.FireIntensity = 0.5;
                }

                if (building.Damage >= 1.0)
                {
                    building.IsDestroyed = true;
                    building.IsOnFire = false;
                    BuildingsDestroyed++;
                }
            }
        }

        // Evacuation trigger
        TriggerEvacuation(impactPosition, impactRadius * 3);
    }

    public void Draw(DrawingContext context)
    {
        var cityBrush = new SolidColorBrush(Color.FromArgb(40, 100, 200, 100));
        context.DrawEllipse(cityBrush, new Pen(new SolidColorBrush(Colors.Green), 3),
            new Point(Center.X, Center.Y), Radius, Radius);
            
       
        foreach (var building in Buildings)
        {
            if (building.IsDestroyed)
            {
                var ruinBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                var ruinHeight = building.Bounds.Height * 0.3;
                var ruinRect = new Rect(
                    building.Bounds.Left,
                    building.Bounds.Bottom - ruinHeight,
                    building.Bounds.Width,
                    ruinHeight);
                context.DrawRectangle(ruinBrush, new Pen(Brushes.Black, 1), ruinRect);
                continue;
            }

            var buildingBrush = new SolidColorBrush(building.Color);

            if (building.Damage > 0)
            {
                var damageFactor = (byte)(255 * (1 - building.Damage * 0.5));
                buildingBrush = new SolidColorBrush(Color.FromRgb(
                    (byte)(building.Color.R * damageFactor / 255),
                    (byte)(building.Color.G * damageFactor / 255),
                    (byte)(building.Color.B * damageFactor / 255)));
            }

            context.DrawRectangle(buildingBrush, new Pen(Brushes.Black, 1), building.Bounds);

            // Огонь на здании
            if (building.IsOnFire)
            {
                var fireAlpha = (byte)(Math.Sin(DateTime.Now.Millisecond * 0.01) * 100 + 155);
                var fireBrush = new SolidColorBrush(Color.FromArgb(fireAlpha, 255, (byte)(100 + building.FireIntensity * 155), 0));

                for (int i = 0; i < 3; i++)
                {
                    var fireX = building.Bounds.Left + (building.Bounds.Width / 4) * (i + 0.5);
                    var fireY = building.Bounds.Top + 5;
                    context.DrawEllipse(fireBrush, null, new Point(fireX, fireY), 5, 8);
                }
            }

            var windowBrush = new SolidColorBrush(Color.FromArgb(180, 255, 255, 150));
            for (double y = building.Bounds.Top + 5; y < building.Bounds.Bottom - 5; y += 10)
            {
                for (double x = building.Bounds.Left + 5; x < building.Bounds.Right - 5; x += 8)
                {
                    context.DrawRectangle(windowBrush, null, new Rect(x, y, 3, 5));
                }
            }

            if (building.IsTower && !building.IsDestroyed)
            {
                var antennaTop = new Point(
                    building.Bounds.Center.X,
                    building.Bounds.Top - 15);

                context.DrawLine(
                    new Pen(new SolidColorBrush(Colors.Red), 2),
                    new Point(building.Bounds.Center.X, building.Bounds.Top),
                    antennaTop);

                var blinkAlpha = (byte)(Math.Sin(DateTime.Now.Millisecond * 0.01) * 127 + 128);
                var blinkBrush = new SolidColorBrush(Color.FromArgb(blinkAlpha, 255, 0, 0));
                context.DrawEllipse(blinkBrush, null, antennaTop, 3, 3);
            }
        }

        // Бункеры
        foreach (var bunker in Bunkers)
        {
            var bunkerColor = bunker.IsOverloaded ? Colors.Red : Color.FromRgb(70, 70, 70);
            var bunkerBrush = new SolidColorBrush(bunkerColor);
            context.DrawRectangle(bunkerBrush, new Pen(new SolidColorBrush(Colors.Yellow), 2), bunker.Bounds);

            context.DrawLine(new Pen(new SolidColorBrush(Colors.Yellow), 2),
                new Point(bunker.Position.X - 10, bunker.Position.Y),
                new Point(bunker.Position.X + 10, bunker.Position.Y));
            context.DrawLine(new Pen(new SolidColorBrush(Colors.Yellow), 2),
                new Point(bunker.Position.X, bunker.Position.Y - 10),
                new Point(bunker.Position.X, bunker.Position.Y + 10));

            var capacityColor = bunker.IsOverloaded ? Brushes.Red : Brushes.White;
            var capacityText = new FormattedText(
                $"{bunker.CurrentOccupants}/{bunker.Capacity}",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                9,
                capacityColor);
            context.DrawText(capacityText, new Point(bunker.Position.X - 15, bunker.Position.Y + 18));
        }

        foreach (var vehicle in Vehicles)
        {
            if (!vehicle.IsActive) continue;

            var vehicleBrush = new SolidColorBrush(vehicle.Color);
            context.DrawRectangle(vehicleBrush, new Pen(Brushes.Black, 1),
                new Rect(vehicle.Position.X - 4, vehicle.Position.Y - 3, 8, 6));

            if (vehicle.IsAmbulance)
            {
                // Красный крест
                context.DrawLine(new Pen(Brushes.Red, 2),
                    new Point(vehicle.Position.X - 2, vehicle.Position.Y),
                    new Point(vehicle.Position.X + 2, vehicle.Position.Y));
                context.DrawLine(new Pen(Brushes.Red, 2),
                    new Point(vehicle.Position.X, vehicle.Position.Y - 2),
                    new Point(vehicle.Position.X, vehicle.Position.Y + 2));
            }

            context.DrawEllipse(new SolidColorBrush(Colors.Yellow), null,
                new Point(vehicle.Position.X + 4, vehicle.Position.Y), 1.5, 1.5);
        }

        foreach (var person in People)
        {
            var personBrush = new SolidColorBrush(person.Color);

            if (person.IsEvacuating)
            {
                var alpha = (byte)(Math.Sin(DateTime.Now.Millisecond * 0.02 + person.PanicLevel * 10) * 100 + 155);
                personBrush = new SolidColorBrush(Color.FromArgb(alpha, 255, 50, 50));

                var evacuationPen = new Pen(new SolidColorBrush(Color.FromArgb(100, 255, 255, 0)), 1)
                {
                    DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
                };
                context.DrawLine(evacuationPen,
                    new Point(person.Position.X, person.Position.Y),
                    new Point(person.EvacuationTarget.X, person.EvacuationTarget.Y));
            }

            context.DrawEllipse(personBrush, null,
                new Point(person.Position.X, person.Position.Y), 2.5, 2.5);

            context.DrawEllipse(personBrush, null,
                new Point(person.Position.X, person.Position.Y - 3), 1.5, 1.5);
        }

        foreach (var explosion in Explosions)
        {
            var alpha = (byte)(explosion.Life * 255);

            var color1 = explosion.WasIntercepted ?
                Color.FromArgb(alpha, 0, 255, 255) :
                Color.FromArgb(alpha, 255, 100, 0);
            var outerBrush = new SolidColorBrush(color1);
            context.DrawEllipse(outerBrush, null,
                new Point(explosion.Position.X, explosion.Position.Y),
                explosion.Size, explosion.Size);

            var color2 = explosion.WasIntercepted ?
                Color.FromArgb(alpha, 100, 255, 255) :
                Color.FromArgb(alpha, 255, 200, 0);
            var innerBrush = new SolidColorBrush(color2);
            context.DrawEllipse(innerBrush, null,
                new Point(explosion.Position.X, explosion.Position.Y),
                explosion.Size * 0.6, explosion.Size * 0.6);

            var color3 = explosion.WasIntercepted ?
                Color.FromArgb(alpha, 200, 255, 255) :
                Color.FromArgb(alpha, 255, 255, 200);
            var centerBrush = new SolidColorBrush(color3);
            context.DrawEllipse(centerBrush, null,
                new Point(explosion.Position.X, explosion.Position.Y),
                explosion.Size * 0.3, explosion.Size * 0.3);

            if (explosion.CasualtiesCount > 0)
            {
                var casualtyText = new FormattedText(
                    $" {explosion.CasualtiesCount}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    16,
                    new SolidColorBrush(Color.FromArgb((byte)(explosion.Life * 255), 255, 50, 50)));
                context.DrawText(casualtyText, new Point(explosion.Position.X - 25, explosion.Position.Y - 45));
            }
        }
    }
}