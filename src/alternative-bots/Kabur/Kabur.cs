using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Kabur : Bot
{   
    static void Main(string[] args)
    {
        new Kabur().Start();
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

    


    Kabur() : base(BotInfo.FromFile("Kabur.json")) { }

    Dictionary<int, EnemyBot> enemies = new Dictionary<int, EnemyBot>();
    
    const double MOVE_DISTANCE = 100;
    double prevFx = 0, prevFy = 0;
    int Width = 800, Height = 600;
    Random rand = new Random();

    public override void Run()
    {
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        int currEnemies = EnemyCount;
        // AdjustRadarForBodyTurn = true;
        BodyColor = Color.Gray;

        while (IsRunning)
        {
            SetTurnRadarRight(360);
            if (currEnemies > EnemyCount) currEnemies = EnemyCount;
            (double totalFx, double totalFy) = CalculateTotalEnemyForce();

            double smoothing = 0.8;
            prevFx = smoothing * prevFx + (1 - smoothing) * totalFx;
            prevFy = smoothing * prevFy + (1 - smoothing) * totalFy;

            double forceMagnitude = Math.Sqrt(prevFx * prevFx + prevFy * prevFy);
            if (forceMagnitude < 0.1)
            {
                SetTurnRight(45 + rand.NextDouble() * 90);
                SetForward(MOVE_DISTANCE / 2);
                Go();
                continue;
            }

            double angle = Math.Atan2(prevFy, prevFx);
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
            // Wall Avoidance
            if (X < 50 || X > 750 || Y < 50 || Y > 550) {
                SetTurnRight(90);
                SetBack(MOVE_DISTANCE);
}

            Go();
        }
    }

    public double getFirepower(double enemyDistace){
        return Math.Min(3, Math.Max(Energy * 2 / (enemyDistace*0.1),1));
    }
    public void FiringLogic(double X, double Y){
        double eDirection = DirectionTo(X, Y);
        double turnAngle = NormalizeRelativeAngle(eDirection - GunDirection);
        SetTurnGunLeft(turnAngle);
        double firepower = getFirepower(DistanceTo(X,Y));
        if (Energy > 20)
            SetFire(firepower);
        Go();
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

            double force = 9000 * energyFactor / distanceSq;

            double angle = Math.Atan2(dy, dx);
            totalFx += force * Math.Cos(angle);
            totalFy += force * Math.Sin(angle);
        }

        // Force dari wall
        double wallMargin = 60;
        double wallForce = 10000;

        if (X < wallMargin)
            totalFx += wallForce / (X * X); // push right
        else if (X > Width - wallMargin)
            totalFx -= wallForce / ((Width - X) * (Width - X));

        if (Y < wallMargin)
            totalFy += wallForce / (Y * Y); // push down
        else if (Y > Height - wallMargin)
            totalFy -= wallForce / ((Height - Y) * (Height - Y));

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
        FiringLogic(e.X, e.Y);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        FiringLogic(e.X,e.Y);
    }


    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnRight(90);
        SetBack(100);
        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemies.Remove(e.VictimId);
    }

    /* Read the documentation for more events and methods */
}
