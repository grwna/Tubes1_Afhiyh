using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hnfadtya : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    private List<EnemyBot> ListOfEnemy = new List<EnemyBot>();

    static void Main(string[] args)
    {
        new Hnfadtya().Start();
    }

    Hnfadtya() : base(BotInfo.FromFile("Hnfadtya.json")) { }

    public override void Run()
    {
        BodyColor = Color.Blue;
        TurretColor = Color.Blue;
        RadarColor = Color.Black;
        ScanColor = Color.Yellow;
        // SetTurnLeft(180);

        while (IsRunning) {
            SetForward(10);
            SetTurnRadarLeft(360);
            AdjustRadarForGunTurn = true; 
            // int gunIncrement = 3;
            // for (int i = 0; i < 30; i++)
            // {
            //     SetTurnRadarRight(gunIncrement);
            // }
            // gunIncrement *= -1;

            // AdjustGunForBodyTurn = true; 

            // SetTurnLeft(10);
            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double X = e.X;
        double Y = e.Y;
        double direction = e.Direction;
        int id = e.ScannedBotId;

        EnemyBot existingEnemy = new EnemyBot(0, 0, 0, 0, -1);
        EnemyBot target = new EnemyBot(0, 0, 0, 0, -1);

        double minDistance = double.MaxValue;

        // Cari bot dalam list berdasarkan ID
        foreach (var enemy in ListOfEnemy) {
            if (enemy.Id == id) {
                existingEnemy = enemy;
                break;
            }
        }        

        if (existingEnemy == null) {
            ListOfEnemy.Add(new EnemyBot(X, Y, distance, direction, id));            
        } else {
            ListOfEnemy.Remove(existingEnemy);
            ListOfEnemy.Add(new EnemyBot(X, Y, distance, direction, id));
        }

        foreach (var enemy in ListOfEnemy) {
            if (enemy.Distance < minDistance) {
                minDistance = enemy.Distance;
                target = enemy;
                break;
            }
        }        
        if (target != null){            
            // double turnGunAngle = NormalizeRelativeAngle(target.Direction - GunDirection);
            // SetTurnGunLeft(turnGunAngle);
            // SetFire(1);          

            double turnBodyAngle = NormalizeRelativeAngle(target.Direction - Direction);
            SetTurnLeft(turnBodyAngle);
            SetFire(1);          

            double targetDistance = DistanceTo(target.X, target.Y);      

            if (targetDistance < 50){
                SetTurnLeft(45);
                SetForward(100);
            } else {
                SetForward(targetDistance + 200);    // Nge ram
            }
            SetTurnRadarLeft(0);
            // SetForward(targetDistance + 100); // asumsi target terkejar

            Go(); 
            // double power = Math.Min(3, (targetEnergy*0.15)); // Fire(3) jika target.Energy > 20
        }
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        EnemyBot deadEnemy = new EnemyBot(0, 0, 0, 0, -1);
        foreach (var enemy in ListOfEnemy) {
            if (enemy.Id == e.VictimId) {
                deadEnemy = enemy;
                break;
            }
        } 
        ListOfEnemy.Remove(deadEnemy);
    }
    public override void OnHitBot(HitBotEvent e)
    {
        SetFire(2);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnRight(90);
        SetForward(20);
        Go();
    }


}

public class EnemyBot {
    public double X { get; set; }
    public double Y { get; set; }
    public double Distance { get; set; }
    public double Direction { get; set; }
    public int Id { get; set; }

    public EnemyBot(double x, double y, double distance, double direction, int id) {
        X = x;
        Y = y;
        Distance = distance;
        Direction = direction;
        Id = id;
    }

    public void UpdateBot(double x, double y, double distance, double direction, int id) {
        X = x;
        Y = y;
        Distance = distance;
        Direction = direction;
        Id = id;
    }
}