using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hnfadtya : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    private List<EnemyBot> ListOfEnemy = new List<EnemyBot>();
    EnemyBot target = new EnemyBot(0, 0, double.MaxValue, double.MaxValue, -1);
    int targetId = -1;
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
        // SetTurnRadarLeft(360);                

        while (IsRunning) {
            // if (Speed == 0) {TurnRadarLeft(360);}
            SetForward(2000);
            SetTurnRight(10);
            SetTurnGunRight(20);
            Go();

            // AdjustGunForBodyTurn = true; 

        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double X = e.X;
        double Y = e.Y;
        double direction = e.Direction;
        int id = e.ScannedBotId;
        int index = -1;
        EnemyBot existingEnemy = new EnemyBot(0, 0, 0, 0, -1);

        for (int i = 0; i < ListOfEnemy.Count; i++) {
            if (ListOfEnemy[i].Id == id) {
                ListOfEnemy[i].X = X;
                ListOfEnemy[i].Y = Y;
                ListOfEnemy[i].Distance = distance;
                ListOfEnemy[i].Direction = direction;
                index = i;      
                break; 
            }
        }
        if (index == -1) {
            ListOfEnemy.Add(new EnemyBot(X, Y, distance, direction, id));            
            index = ListOfEnemy.Count - 1;
        }

        if (ListOfEnemy[index].Distance < target.Distance) {
            targetId = ListOfEnemy[index].Id;
        }     

        if (targetId == id) {
            var bearingFromGun = GunBearingTo(target.X, target.Y);
            // TurnGunLeft(bearingFromGun);

            if (Math.Abs(bearingFromGun) <= 3 && GunHeat == 0)
                Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));

            // double turnBodyAngle = NormalizeRelativeAngle(target.Direction - Direction);
            // double targetDistance = DistanceTo(target.X, target.Y);      
            if (bearingFromGun == 0)
                Rescan();
            // ChasingTarget(turnBodyAngle, targetDistance);            
        }
        // if (target.Id != -1 && ListOfEnemy.Count == EnemyCount){  
        // }

    }
    private void ChasingTarget(double TargetDirection, double TargetDistance)
    {
        SetTurnLeft(TargetDirection);
        SetForward(TargetDistance);
        Go();
    }

    // public virtual void OnBulletHitWall(BulletHitWallEvent bulletHitWallEvent) {
    //     if ((bulletHitWallEvent.bullet).ownerid == MyId) {
    //         Rescan();
    //     }
    // }

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