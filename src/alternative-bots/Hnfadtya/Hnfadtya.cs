using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hnfadtya : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    private EnemyBot target = new EnemyBot(0, 0, 0, 0, 0);
    // private int lockedTargetID;
    private bool isTargeting = false;

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
        AdjustGunForBodyTurn = true; // Gun bergerak independen terhadap Body
        // AdjustRadarForGunTurn = true; 
        // AdjustRadarForBodyTurn = true; 

        while (IsRunning) {
            // SetForward(40);
            SetTurnRadarRight(360);
            WaitFor(new TurnCompleteCondition(this));
            isTargeting = true;
            int turnDirection;
            // double targetDistance, turnBodyAngle, turnGunAngle, turnRadarAngle;

            while (isTargeting && target.Energy >= 0)
            {            
                // double bearingFromBody = BearingTo(target.X, target.Y);
                // double bearingFromGun = GunBearingTo(target.X, target.Y); // tadinya var bukan double
                // double bearingFromRadar = RadarBearingTo(target.X, target.Y); // tadinya var bukan double
                double turnRadarAngle = NormalizeRelativeAngle(target.Direction - RadarDirection);
                SetTurnRadarLeft(turnRadarAngle);

                double turnGunAngle = NormalizeRelativeAngle(target.Direction - GunDirection);
                SetTurnGunLeft(turnGunAngle);

                if (turnBodyAngle >= 0)
                    turnDirection = 1;
                else {
                    turnDirection = -1;
                }

                double turnBodyAngle = NormalizeRelativeAngle(target.Direction - Direction);
                SetTurnLeft(turnBodyAngle * turnDirection);

                double targetDistance = DistanceTo(target.X, target.Y);        
                SetForward(targetDistance + 10); // asumsi target terkejar

            }
            Go();
            // isTargeting = false;
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double X = e.X;
        double Y = e.Y;
        double direction = e.Direction;
        double energy = e.Energy;

        if (!isTargeting && distance < target.Distance) {
            target = new EnemyBot(X, Y, distance, direction, energy);
        } 
        else if (isTargeting) {
            target.UpdateBot(X, Y, distance, direction, energy);
            if (target.Energy <= 0)
            {
                isTargeting = false;
            } else {
                SetFire(2);          
            }
            // double power = Math.Min(3, (targetEnergy*0.15)); // Fire(3) jika target.Energy > 20
        }
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
    public double Energy { get; set; }

    public EnemyBot(double x, double y, double distance, double direction, double energy) {
        X = x;
        Y = y;
        Distance = distance;
        Direction = direction;
        Energy = energy;
    }

    public void UpdateBot(double x, double y, double distance, double direction, double energy) {
        X = x;
        Y = y;
        Distance = distance;
        Direction = direction;
        Energy = energy;
    }
}

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        // turn is complete when the remainder of the turn is zero
        return bot.TurnRemaining == 0;
    }
}