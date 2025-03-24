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
        BodyColor   = Color.FromArgb(0xFF, 0x33, 0x33, 0x33);   // abu-abu gelap
        GunColor    = Color.FromArgb(0xFF, 0x66, 0x33, 0x00);   // coklat tua
        TurretColor = Color.FromArgb(0xFF, 0x66, 0x33, 0x00);   // coklat tua
        RadarColor  = Color.FromArgb(0xFF, 0xCC, 0x22, 0x22);   // merah 
        ScanColor   = Color.FromArgb(0xFF, 0xCC, 0x22, 0x22);   // merah
        BulletColor = Color.FromArgb(0xFF, 0xCC, 0x22, 0x22);   // merah

        int enemyCount = EnemyCount;

        while (IsRunning)
        {
            MaxSpeed = 6.0;
            if (enemyCount > EnemyCount)
            {
                enemyCount = EnemyCount;
            }
            targetedId = findWeakestBot();
            SetForward(distance);
            SetTurnRight(10);
            SetTurnGunRight(20);
            Go();
        }
    }

    int findWeakestBot()
    {
        int weakestId = -1;
        double lowestEnergy = double.MaxValue;
        foreach (var entry in scannedBots)
        {
            if (entry.Value.energy < lowestEnergy)
            {
                lowestEnergy = entry.Value.energy;
                weakestId = entry.Key;
            }
        }
        return weakestId;
    }


    public void calculatedFirepower(double enemyEnergy)
    {
        double ratio = (enemyEnergy - 20) / (100 - 20);
        double rawFirepower = 3.0 - ratio * (3.0 - 1.01);
        double firepower = Math.Min(rawFirepower, 3.0);

        SetFire(firepower);  
        Go();  
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (scannedBots.ContainsKey(e.ScannedBotId))
        {
            scannedBots[e.ScannedBotId] = new ScannedBot(e.X, e.Y, e.Energy, DistanceTo(e.X, e.Y));
        }
        else
        {
            scannedBots.Add(e.ScannedBotId, new ScannedBot(e.X, e.Y, e.Energy, DistanceTo(e.X, e.Y)));
        }

        if (targetedId == e.ScannedBotId)
        {
            calculatedFirepower(scannedBots[e.ScannedBotId].energy);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double deltaX = e.X - X;
        double deltaY = e.Y - Y;
        
        double absoluteBearing = Math.Atan2(deltaX, deltaY) * (180.0 / Math.PI);
        absoluteBearing = (absoluteBearing + 360) % 360;

        double relativeBearing = NormalizeRelativeAngle(absoluteBearing - Direction);

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

        SetFire(3);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        // Console.WriteLine("Ouch! I hit a wall, must turn back!");
        
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        scannedBots.Remove(e.VictimId);
        targetedId = -1;
    }
}
