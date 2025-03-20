using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hnfadtya : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    // private EnemyBot? target; // bisa bernilai null (pake '?')
    private EnemyBot target = new EnemyBot(0, 0, double.MaxValue, double.MaxValue);
    // private bool hitTargetCondition = false;

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

        while (IsRunning) {
            ScanEnemies();  

            while (target != null) {
                TargetingEnemy();  
            }

            Go();
        }
    }

    public void ScanEnemies()
    {
        SetTurnRadarRight(180);
        Go();
    }
    
    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double X = e.X;
        double Y = e.Y;
        double direction = e.Direction;


        if (target == null || distance < target.Distance)
        {
            target = new EnemyBot(X, Y, distance, direction);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnRight(45);
        SetForward(20);
        SetTurnRight(45);
        SetForward(30);
        Go();
    }

    public void TargetingEnemy() {
        if (target == null) return;

        double angleEnemy = BearingTo(target.X, target.Y);
        double directionEnemy = target.Direction;
        double distanceEnemy = target.Distance;

        SetTurnGunRight(angleEnemy + directionEnemy);
        Go();
        double power = Math.Min(1.5, 400 / target.Distance); 
        Fire(power);

        SetTurnRight(angleEnemy + directionEnemy);
        Go();

        SetTurnRadarRight(angleEnemy + directionEnemy);
        Go();

    

        SetForward(100);
        Go();
        Fire(power);
    }

    // public void FireAtTarget() {
    //     if (target == null) return;

    //     double angleEnemy = BearingTo(target.X, target.Y);
    //     Fire(power);
    //     Fire(power);    
    // }

    // public override void isEnemyGotHit(BulletHitBotEvent e)
    // {
    //     if (e.damage != 0)
    //     {
    //         hitTargetCondition = true;
    //     }
    // }
}

class EnemyBot {
    public double X { get; set; }
    public double Y { get; set; }
    public double Distance { get; set; }
    public double Direction { get; set; }

    public EnemyBot(double x, double y, double distance, double direction) {
        X = x;
        Y = y;
        Distance = distance;
        Direction = direction;
    }
}
