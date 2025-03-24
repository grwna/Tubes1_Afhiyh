using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hnfadtya : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    private List<EnemyBot> ListOfEnemy = new List<EnemyBot>();
    bool isScanning = false;
    int turnDirection = 1; // clockwise (-1) or counterclockwise (1)


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
        SetTurnRadarLeft(360);                

        while (IsRunning) {
            if (Speed == 0) {TurnRadarLeft(360);}

            AdjustGunForBodyTurn = true; 
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

        if (target != null && ListOfEnemy.Count == EnemyCount){  
            var bearingFromGun = GunBearingTo(target.X, target.Y);
            TurnGunLeft(bearingFromGun);

            if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
                Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));

            // if (bearingFromGun == 0)

            double turnBodyAngle = NormalizeRelativeAngle(target.Direction - Direction);
            double targetDistance = DistanceTo(target.X, target.Y);      
            ChasingTarget(turnBodyAngle, targetDistance);
            Rescan();
        }
    }
    private void ChasingTarget(double TargetDirection, double TargetDistance)
    {
        SetTurnLeft(TargetDirection);
        SetForward(TargetDistance);
        Go();
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
        var bearingFromGun = GunBearingTo(e.X, e.Y);

        if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
            Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));

        if (bearingFromGun == 0)
            Rescan();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Back(10);
        TurnRight(90);
        Forward(20);
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
}