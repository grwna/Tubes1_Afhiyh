using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/*
GrwnaBOT
Greedy: Focuses on killing one tank without backing down under any circumstances

*/
public class grwna : Bot
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
        } else {
            SetForward(500);
        }
        Go();
    }

    static void Main(string[] args)
    {
        new grwna().Start();
    }

    // Constructor, which loads the bot settings file
    grwna() : base(BotInfo.FromFile("grwna.json")) { }

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
                SetForward(20);
                SetTurnRadarLeft(360);
            }
            AdjustGunForBodyTurn = true; 
            Go();
        }
    }

    // We scanned another bot -> fire!
   public override void OnScannedBot(ScannedBotEvent e)
    {
        // isIdle = false;
        // TurnRadarLeft(CalcDeltaAngle(e.Direction, RadarDirection));
        // Console.WriteLine("Scanned bot energy: " + e.Energy);

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
        // e = Enemy
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
        // Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    /* Read the documentation for more events and methods */
    public override void OnHitByBullet(HitByBulletEvent e)
    {   
        isIdle = false;
        SetTurnLeft(90);
        SetForward(200);
        isIdle = true;
        Go();
    }
}