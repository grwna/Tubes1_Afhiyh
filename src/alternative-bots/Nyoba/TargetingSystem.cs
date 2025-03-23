using System;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;

public class TargetingSystem
{
    private Bot bot;

    public TargetingSystem(Bot bot)
    {
        this.bot = bot;
    }

    // Try to fire at the closest enemy
    public void TryFireAtClosestEnemy(Dictionary<int, EnemyBot> enemies)
    {
        if (enemies.Count == 0) return;
        
        // Find closest enemy
        double minDistance = double.MaxValue;
        EnemyBot closestEnemy = new EnemyBot();
        
        foreach (var enemy in enemies.Values)
        {
            double dx = enemy.X - bot.X;
            double dy = enemy.Y - bot.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }
        
        // Calculate firing angle
        double enemyX = closestEnemy.X;
        double enemyY = closestEnemy.Y;
        double absDegrees = Math.Atan2(enemyY - bot.Y, enemyX - bot.X) * 180 / Math.PI;
        
        // Convert to relative angle
        double relDegrees = Bot.NormalizeRelativeAngle(absDegrees - bot.Heading);
        
        // Turn gun to enemy
        bot.SetTurnGunRight(relDegrees);
        
        // Determine firing power based on distance
        double firePower = DetermineFiringPower(minDistance);
        
        // Only fire if we have enough energy and gun is pointing at enemy
        if (bot.Energy > firePower * 4 && Math.Abs(bot.GunHeading - absDegrees) < 3)
        {
            bot.SetFire(firePower);
        }
    }

    // Determine firing power based on distance
    private double DetermineFiringPower(double distance)
    {
        if (distance < 100) return 3;
        if (distance < 200) return 2;
        if (distance < 400) return 1.5;
        if (distance < 600) return 1;
        return 0.5;
    }
}