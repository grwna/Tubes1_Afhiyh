using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Hnfadtya : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    private EnemyBot target = new EnemyBot(0, 0, double.MaxValue, double.MaxValue, -1);
    // private int lockedTargetID;
    int scannedEnemies = 0;
    bool isScanning = true;

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
        // WaitFor(new TurnCompleteCondition(this));
        // Console.WriteLine("im scanning by radar");
        AdjustRadarForGunTurn = true; 
        AdjustRadarForBodyTurn = true; 

        while (IsRunning) {
            // SetForward(10);
            // SetTurnLeft(10);
            if (scannedEnemies < EnemyCount && isScanning){
                SetTurnRadarLeft(360);
                isScanning = false
            } else {
                AdjustRadarForGunTurn = false; 
                AdjustRadarForBodyTurn = false; 
                AdjustGunForBodyTurn = true; // Gun bergerak independen terhadap Body
                double turnGunAngle = NormalizeRelativeAngle(target.Direction - GunDirection);
                SetTurnGunLeft(turnGunAngle);
            }

            // for (int i = 0; i < 30; i++)
            // {
            //     SetTurnGunRight(gunIncrement);
            // }
            // gunIncrement *= -1;
            // if (isScanning)
            // {
            //     SetForward(40);
            // }
            // double targetDistance, turnBodyAngle, turnGunAngle, turnRadarAngle;
            Go();
            // isScanning = false;
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double X = e.X;
        double Y = e.Y;
        double direction = e.Direction;
        int id = e.ScannedBotId;
        if (scannedEnemies < EnemyCount){
            scannedEnemies++;
        }

        if (scannedEnemies <= EnemyCount && distance < target.Distance) {
            target = new EnemyBot(X, Y, distance, direction, id);
        } 
        else if (scannedEnemies == EnemyCount && target.Id == e.Id) {
            // target.UpdateBot(X, Y, distance, direction, energy);
            // SetForward(10);
            // double bearingFromBody = BearingTo(target.X, target.Y);
            // double bearingFromGun = GunBearingTo(target.X, target.Y); // tadinya var bukan double
            // double bearingFromRadar = RadarBearingTo(target.X, target.Y); // tadinya var bukan double

            // double turnRadarAngle = NormalizeRelativeAngle(target.Direction - RadarDirection);
            // SetTurnRadarLeft(turnRadarAngle);

            double turnGunAngle = NormalizeRelativeAngle(e.Direction - GunDirection);
            SetTurnGunLeft(turnGunAngle);
            SetFire(2);          

            double turnBodyAngle = NormalizeRelativeAngle(e.Direction - Direction);
            // int turnBodyDirection;
            // if (turnBodyAngle >= 0)
            //     turnBodyDirection = 1;
            // else {
            //     turnBodyDirection = -1;
            // }
            SetTurnLeft(turnBodyAngle);

            double targetDistance = DistanceTo(e.X, e.Y);        
            SetForward(targetDistance); // asumsi target terkejar

            // SetTurnRadarLeft(0);
            Go();   
            // double power = Math.Min(3, (targetEnergy*0.15)); // Fire(3) jika target.Energy > 20
        }
    }
    public override void OnBotDeath(BotDeathEvent e)
    {
        if (e.VictimId == target.Id){
            scannedEnemies--;
            isScanning = true;
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

    // public void IsScanningComplete() {
    //     return (bot - Direction) == 0;
    // }
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