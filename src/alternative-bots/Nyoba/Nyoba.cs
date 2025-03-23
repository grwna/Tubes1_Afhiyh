using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Nyoba : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new Nyoba().Start();
    }

    public struct EnemyBot
    {
        public double X;
        public double Y;
        public double Energy;
        public double Direction;

        public EnemyBot(double x, double y, double energy, double direction)
        {
            X = x;
            Y = y;
            Energy = energy;
            Direction = direction;
        }
    }

    


    Nyoba() : base(BotInfo.FromFile("Nyoba.json")) { }

    Dictionary<int, EnemyBot> enemies = new Dictionary<int, EnemyBot>();
    
    const double MOVE_DISTANCE = 100;

    public override void Run()
    {
        movement = Double.PositiveInfinity;
        int currEnemies = EnemyCount;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        // AdjustRadarForBodyTurn = true;
        BodyColor = Color.Gray;

        while (IsRunning)
        {
            SetTurnRadarRight(Double.PositiveInfinity);
            if (currEnemies > EnemyCount) currEnemies = EnemyCount;
            (double totalFx, double totalFy) = CalculateTotalEnemyForce();
            double angle = Math.Atan2(totalFy, totalFx);
            angle = NormalizeRelativeAngle(angle * 180 / Math.PI);
            if (Math.Abs(angle) < 90)
            {
                SetTurnRight(angle);
                SetForward(MOVE_DISTANCE);
            }
            else
            {
                SetTurnRight(NormalizeRelativeAngle(angle + 180));
                SetBack(MOVE_DISTANCE);
            }
            Go();
            // SetForward(-100); 
            // SetFire(1);
            // SetTurnRight(-90);
            // SetTurnRadarRight(360);
        }
    }

    private (double totalFx, double totalFy) CalculateTotalEnemyForce()
    {
        double totalFx = 0, totalFy = 0;
        foreach (var enemy in enemies.Values)
        {
            double dx = enemy.X - X;
            double dy = enemy.Y - Y;
            double distanceSq = dx * dx + dy * dy;
            if (distanceSq < 1) distanceSq = 1;

            double energyFactor = enemy.Energy / 100;

            double force = 8000 * energyFactor / distanceSq;

            double angle = Math.Atan2(dy, dx);
            totalFx += force * Math.Cos(angle);
            totalFy += force * Math.Sin(angle);
        }
        return (totalFx, totalFy);
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (enemies.ContainsKey(e.ScannedBotId))
        {
            enemies[e.ScannedBotId] = new EnemyBot(e.X, e.Y, e.Energy, e.Direction);
        }
        else
        {
            enemies.Add(e.ScannedBotId, new EnemyBot(e.X, e.Y, e.Energy, e.Direction));
        }
        // Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemies.Remove(e.VictimId);
    }

    /* Read the documentation for more events and methods */
}
