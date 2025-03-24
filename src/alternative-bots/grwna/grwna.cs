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

    public double getFirepower(double enemyDistace){
        return Math.Min(3, Math.Max(Energy * 2 / (enemyDistace*0.1),1));
    }
    // The main method starts our bot


    public void circleEnemyLogic(double eDistance, double eDirection){
        double turnAngle = NormalizeRelativeAngle(eDirection - Direction);
        SetTurnLeft(turnAngle);
        if (eDistance < 50){
            SetTurnLeft(45);
            SetForward(100);
            SetTurnLeft(45);
        } else {
            SetForward(500);    // Nge ram
        }
        if (Speed == 0 && !(X < 20 || X > 780) && !(Y < 20 || Y > 580)){
            SetTurnLeft(90);
        }
        Go();
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
        BodyColor = Color.FromArgb(0xFF, 0xFF,  0xFF);   // orange
        GunColor = Color.FromArgb(0xFF,  0x00,  0x00);    // dark orange
        TurretColor = Color.FromArgb(0xFF,  0x00,  0x00); // dark orange
        RadarColor = Color.FromArgb(0xFF,  0xFF,  0xFF);  // red
        ScanColor = Color.FromArgb(0xFF,  0xFF,  0xFF);   // red
        BulletColor = Color.FromArgb( 0xFF,  0x00, 0x00); // light blue

        // Spin the gun around slowly... forever
        while (IsRunning)
        {   
            if (Speed == 0){isIdle = true;}
            if (Energy < 7){
                for (int i = 0; i < 5; i++){
                    SetTurnLeft(45);
                    SetForward(50);
                    SetTurnLeft(45);
                    SetForward(50);
                    SetTurnLeft(45);
                    SetForward(50);
                }
            }
            if (isIdle){
                SetTurnLeft(90);
                SetBack(100);
                SetTurnRadarLeft(360);
            }
            AdjustGunForBodyTurn = true; 
            Go();
        }
    }

    // We scanned another bot -> fire!
   public override void OnScannedBot(ScannedBotEvent e)
    {

        double eDirection = DirectionTo(e.X, e.Y);
        double turnAngle = NormalizeRelativeAngle(eDirection - GunDirection);
        SetTurnGunLeft(turnAngle);
        double eDistance = DistanceTo(e.X, e.Y);
        double firepower = getFirepower(eDistance);
        SetFire(firepower);
        circleEnemyLogic(eDistance, eDirection);
        SetTurnRadarLeft(0);
        Go();
        
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double eDirection = DirectionTo(e.X, e.Y);
        double turnAngle = NormalizeRelativeAngle(eDirection - GunDirection);
        SetTurnGunLeft(turnAngle);
        double eDistance = DistanceTo(e.X, e.Y);
        double firepower = getFirepower(eDistance);
        SetFire(firepower);
        circleEnemyLogic(eDistance, eDirection);
        SetTurnRadarLeft(0);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetTurnRight(45);
        SetForward(20);
        SetTurnRight(45);
        SetForward(30);
        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {   
        isIdle = false;
        SetTurnLeft(90);
        SetForward(200);
        isIdle = true;
        Go();
    }
}