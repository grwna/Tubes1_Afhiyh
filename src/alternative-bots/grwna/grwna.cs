using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/*
GrwnaBOT
Greedy: Focuses on killing one tank without backing down under any circumstances

*/
public class Grwna : Bot
{
    bool isIdle = true;
    int delayFire = 0;
    class CurrentEnemyBot {
        public static double previousBearing = 0;
        public static double previousX = 0;
        public static double previousY = 0;
    }

    static void Main(string[] args)
    {
        new Grwna().Start();
    }

    // Constructor, which loads the bot settings file
    Grwna() : base(BotInfo.FromFile("Grwna.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        // Set colors
        BodyColor = Color.FromArgb(0xFF, 0xFF,  0xFF);
        GunColor = Color.FromArgb(0xFF,  0x00,  0x00);
        TurretColor = Color.FromArgb(0xFF,  0x00,  0x00);
        RadarColor = Color.FromArgb(0xFF,  0xFF,  0xFF);
        ScanColor = Color.FromArgb(0xFF,  0xFF,  0xFF);
        BulletColor = Color.FromArgb( 0xFF,  0x00, 0x00);

        while (IsRunning)
        {  
            if (Speed == 0){isIdle = true;}
            if (isIdle){
                SetForward(20);
                SetTurnRadarLeft(360);
            }
            AdjustGunForBodyTurn = true; 
            if (delayFire > 0){
                delayFire--;
            }
            Go();
        }
    }

   public override void OnScannedBot(ScannedBotEvent e)
    {
        agressiveAttack(e.X, e.Y);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        // e = Enemy
        agressiveAttack(e.X, e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnRight(45);
        SetForward(20);
        SetTurnRight(45);
        SetForward(30);
        Go();
    }

    public override void OnBulletHitWall(BulletHitWallEvent e)
    {
        delayFire = 5;
    }
    public override void OnHitByBullet(HitByBulletEvent e)
    {   
        isIdle = false;
        SetTurnLeft(90);
        SetForward(200);
        isIdle = true;
        Go();
    }


    public double getFirepower(double enemyDistace){
        return Math.Min(3, Math.Max(Energy * 2 / (enemyDistace*0.2),1));
    }


    public void circleEnemyLogic(double eDistance, double eDirection){
        double turnAngle = NormalizeRelativeAngle(eDirection - Direction);
        SetTurnLeft(turnAngle);
        if (eDistance < 50){
            SetTurnLeft(90);
            SetForward(500);
            Go();
            SetBack(500);
            Go();
        } else {
            SetForward(500);    // Nge ram
            Go();
        }
    }
    
    public void agressiveAttack(double x, double y){
        double eDirection = DirectionTo(x, y);
        double turnAngle = NormalizeRelativeAngle(eDirection - GunDirection);

        // Predictive shooting
        double deltaBearing = NormalizeRelativeAngle(eDirection - CurrentEnemyBot.previousBearing);
        double dx = x - CurrentEnemyBot.previousX;
        double dy = y - CurrentEnemyBot.previousY;
        double enemySpeed = Math.Sqrt(dx * dx + dy * dy);
        if (Math.Abs(deltaBearing) > 5 && enemySpeed > 0)
        {
            turnAngle += deltaBearing * 0.5;
        }

        SetTurnGunLeft(turnAngle);

        double eDistance = DistanceTo(x, y);
        double firepower = getFirepower(eDistance);
        if (delayFire == 0){
            SetFire(firepower);
        }

        circleEnemyLogic(eDistance, eDirection);
        SetTurnRadarLeft(0);

        // Save enemy position & direction
        CurrentEnemyBot.previousBearing = eDirection;
        CurrentEnemyBot.previousX = x;
        CurrentEnemyBot.previousY = y;

        Go();
    }

}