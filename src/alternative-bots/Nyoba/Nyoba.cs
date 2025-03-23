using System;
using System.Drawing;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Nyoba : Bot
{   
    /* A bot that drives using force field algorithm with wall avoidance */
    static void Main(string[] args)
    {
        new Nyoba().Start();
    }

    // Constants for movement and avoidance
    const double MOVE_DISTANCE = 100;
    const double WALL_MARGIN = 100; // Safe distance from walls
    const double WALL_FORCE = 12000; // Force constant for walls
    const double ENEMY_FORCE = 8000; // Force constant for enemies
    const double CORNER_FORCE = 20000; // Extra force to avoid corners

    // Wall avoidance state
    private bool avoidingWall = false;
    private double avoidAngle = 0;
    private int avoidanceCounter = 0;
    private const int MAX_AVOIDANCE_TICKS = 10;

    // Bot components
    private Dictionary<int, EnemyBot> enemies = new Dictionary<int, EnemyBot>();
    private WallAvoidance wallAvoidance;
    private ForceFieldCalculator forceField;
    private TargetingSystem targeting;

    public Nyoba() : base(BotInfo.FromFile("Nyoba.json")) 
    { 
        wallAvoidance = new WallAvoidance(this);
        forceField = new ForceFieldCalculator(this);
        targeting = new TargetingSystem(this);
    }

    public override void Run()
    {
        InitializeBot();

        while (IsRunning)
        {
            SetTurnRadarRight(Double.PositiveInfinity);
            
            if (avoidingWall)
            {
                HandleWallAvoidanceMode();
                continue;
            }
            
            // Calculate forces from enemies and walls
            (double enemyFx, double enemyFy) = forceField.CalculateTotalEnemyForce(enemies, ENEMY_FORCE);
            (double wallFx, double wallFy) = forceField.CalculateWallForce(WALL_MARGIN, WALL_FORCE);
            (double cornerFx, double cornerFy) = forceField.CalculateCornerForce(WALL_MARGIN, CORNER_FORCE);
            
            // Combine all forces and execute movement
            ExecuteMovement(enemyFx + wallFx + cornerFx, enemyFy + wallFy + cornerFy);
            
            // Try to fire at enemies
            targeting.TryFireAtClosestEnemy(enemies);
            
            Go();
        }
    }

    private void InitializeBot()
    {
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        AdjustRadarForBodyTurn = true;
        BodyColor = Color.Gray;
        GunColor = Color.DarkGray;
        RadarColor = Color.DarkBlue;
        BulletColor = Color.Red;
    }

    private void HandleWallAvoidanceMode()
    {
        if (avoidanceCounter++ > MAX_AVOIDANCE_TICKS || !wallAvoidance.IsNearWall(WALL_MARGIN * 0.8))
        {
            avoidingWall = false;
            avoidanceCounter = 0;
        }
        else
        {
            // Continue avoiding in the predetermined direction
            SetTurnRight(avoidAngle);
            SetForward(MOVE_DISTANCE * 0.5); // Move at half speed while avoiding
            
            // Fire at enemies if possible during avoidance
            targeting.TryFireAtClosestEnemy(enemies);
            
            Go();
        }
    }

    private void ExecuteMovement(double totalFx, double totalFy)
    {
        // Convert force to angle
        double angle = Math.Atan2(totalFy, totalFx);
        angle = NormalizeRelativeAngle(angle * 180 / Math.PI);
        
        // Check for potential wall collision
        double safeDistance = MOVE_DISTANCE;
        if (wallAvoidance.WillHitWall(MOVE_DISTANCE, angle))
        {
            // Apply wall smoothing if we're going to hit a wall
            wallAvoidance.ApplyWallSmoothing(ref angle);
            
            // Find a safe distance that won't result in collision
            safeDistance = wallAvoidance.FindMaxSafeDistance(angle, MOVE_DISTANCE);
            safeDistance = Math.Min(MOVE_DISTANCE, safeDistance * 0.8); // 80% of max safe distance
            
            // If we're too close to a wall, enter wall avoidance mode
            if (safeDistance < MOVE_DISTANCE * 0.3)
            {
                avoidingWall = true;
                avoidanceCounter = 0;
                
                // Determine the best angle to avoid the wall
                avoidAngle = wallAvoidance.FindBestAvoidanceAngle(MOVE_DISTANCE);
                
                SetTurnRight(avoidAngle);
                SetForward(MOVE_DISTANCE * 0.5);
                return;
            }
        }
        
        // Execute movement based on calculated angle
        if (Math.Abs(angle) < 90)
        {
            SetTurnRight(angle);
            SetForward(safeDistance);
        }
        else
        {
            SetTurnRight(NormalizeRelativeAngle(angle + 180));
            SetBack(safeDistance);
        }
    }

    // Event handlers
    public override void OnScannedBot(ScannedBotEvent e)
    {
        if (enemies.ContainsKey(e.ScannedBotId))
        {
            enemies[e.ScannedBotId] = new EnemyBot(e.X, e.Y, e.Energy, e.Direction);
        }
        else
        {
            enemies.Add(e.ScannedBotId, new EnemyBot(e.X, e.Y, e.Energy, e.Direction));
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Collision with bot at " + e.X + ", " + e.Y);
        
        // Immediate evasive maneuver on collision
        double angle = Math.Atan2(Y - e.Y, X - e.X) * 180 / Math.PI;
        angle = NormalizeRelativeAngle(angle - Heading);
        
        SetTurnRight(angle);
        SetBack(MOVE_DISTANCE);
        Go();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Hit wall! Emergency maneuver!");
        
        // Enter wall avoidance mode
        avoidingWall = true;
        avoidanceCounter = 0;
        
        // Turn perpendicular to the wall
        avoidAngle = NormalizeRelativeAngle(e.Angle + 90);
        
        SetTurnRight(avoidAngle);
        SetForward(MOVE_DISTANCE * 0.5);
        Go();
    }

    public override void OnBotDeath(BotDeathEvent e)
    {
        enemies.Remove(e.VictimId);
    }
}
