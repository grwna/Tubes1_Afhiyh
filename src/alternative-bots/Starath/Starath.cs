using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Starath : Bot
{   
    struct ScannedBot
    {
        public double X;
        public double Y;
        public double energy;
        public double distance;

        public ScannedBot(double x, double y, double energy, double distance)
        {
            X = x;
            Y = y;
            this.energy = energy;
            this.distance = distance;
        }
    }

    // Make sure to initialize your dictionary.
    Dictionary<int, ScannedBot> scannedBots = new Dictionary<int, ScannedBot>();
    int distance = 40000;
    int targetedId = -1;

    static void Main(string[] args)
    {
        new Starath().Start();
    }

    Starath() : base(BotInfo.FromFile("Starath.json")) { }

    public override void Run()
    {
        // Customize bot colors or any additional settings here.
        while (IsRunning)
        {
            SetForward(distance);
            SetTurnRight(10);
            TurnGunRight(20);
        }
    }


    public void calculatedFirepower(double enemyEnergy)
    {
        double firepower;

        if (enemyEnergy <= 20)
        {
            // If the enemy has very low energy, maximize firepower to finish them off
            firepower = 3.0;
        }
        else if (enemyEnergy >= 100)
        {
            // If the enemy has high energy, use minimal firepower to conserve energy
            firepower = 1.01;
        }
        else
        {
            // Linear interpolation for intermediate enemy energy levels
            double ratio = (enemyEnergy - 20) / (100 - 20);
            firepower = 3.0 - ratio * (3.0 - 1.01);
        }

        // Ensure firepower is within the allowed limits
        firepower = Math.Max(1.01, Math.Min(firepower, 3.0));

        // Fire using the calculated firepower
        Fire(firepower);    
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Add or update the scanned bot information.
        scannedBots[e.ScannedBotId] = new ScannedBot(e.X, e.Y, e.Energy, DistanceTo(e.X, e.Y));

        if (targetedId == -1)
        {
            targetedId = e.ScannedBotId;
        }
        else if (scannedBots[e.ScannedBotId].energy < scannedBots[targetedId].energy)
        {
            targetedId = e.ScannedBotId;
        }

        if (targetedId != -1 || targetedId == e.ScannedBotId)
        {
            calculatedFirepower(scannedBots[e.ScannedBotId].energy);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // Compute the difference between the enemy's position and your bot's position.
        double deltaX = e.X - X;
        double deltaY = e.Y - Y;
        
        // Calculate the absolute bearing from your bot to the enemy (in degrees).
        // Note: Math.Atan2 expects (deltaX, deltaY) and returns the angle in radians.
        double absoluteBearing = Math.Atan2(deltaX, deltaY) * (180.0 / Math.PI);
        absoluteBearing = (absoluteBearing + 360) % 360;  // Normalize to [0, 360)

        // Compute the relative bearing by subtracting your current heading.
        double relativeBearing = NormalizeRelativeAngle(absoluteBearing - Direction);

        // Evasive maneuver: if enemy is roughly in front (< 90°), back off and turn perpendicular.
        if (Math.Abs(relativeBearing) < 90)
        {
            SetBack(150);
            SetTurnRight(90 - relativeBearing);
        }
        else
        {
            SetForward(150);
            SetTurnLeft(90 + relativeBearing);
        }

        // Fire at the enemy.
        calculatedFirepower(e.Energy);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Optional: Add behavior when you hit a wall.
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        // If the target dies, reset the targetedId.
        if (e.VictimId == targetedId)
        {
            int lowestEnergyId = -1;
            double lowestEnergy = double.MaxValue;
            foreach (var entry in scannedBots)
            {
                if (entry.Value.energy < lowestEnergy)
                {
                    lowestEnergy = entry.Value.energy;
                    lowestEnergyId = entry.Key;
                }
            }
            targetedId = lowestEnergyId;
        }
        // Remove the dead bot from the dictionary.
        scannedBots.Remove(e.VictimId);
    }
}
