using System;
using System.Collections.Generic;
using Robocode.TankRoyale.BotApi;

public class ForceFieldCalculator
{
    private Bot bot;

    public ForceFieldCalculator(Bot bot)
    {
        this.bot = bot;
    }

    // Calculate force from all enemies
    public (double totalFx, double totalFy) CalculateTotalEnemyForce(Dictionary<int, EnemyBot> enemies, double enemyForceConstant)
    {
        double totalFx = 0, totalFy = 0;
        foreach (var enemy in enemies.Values)
        {
            double dx = enemy.X - bot.X;
            double dy = enemy.Y - bot.Y;
            double distanceSq = dx * dx + dy * dy;
            if (distanceSq < 1) distanceSq = 1;

            double energyFactor = enemy.Energy / 100;

            double force = enemyForceConstant * energyFactor / distanceSq;

            double angle = Math.Atan2(dy, dx);
            totalFx += force * Math.Cos(angle);
            totalFy += force * Math.Sin(angle);
        }
        return (totalFx, totalFy);
    }

    // Calculate repulsive force from walls
    public (double totalFx, double totalFy) CalculateWallForce(double wallMargin, double wallForceConstant)
    {
        double fx = 0, fy = 0;
        
        // Force from left wall
        double leftWallDistance = bot.X;
        if (leftWallDistance < wallMargin)
            fx += wallForceConstant / Math.Max(1, leftWallDistance * leftWallDistance);
        
        // Force from right wall
        double rightWallDistance = bot.ArenaWidth - bot.X;
        if (rightWallDistance < wallMargin)
            fx -= wallForceConstant / Math.Max(1, rightWallDistance * rightWallDistance);
        
        // Force from top wall
        double topWallDistance = bot.Y;
        if (topWallDistance < wallMargin)
            fy += wallForceConstant / Math.Max(1, topWallDistance * topWallDistance);
        
        // Force from bottom wall
        double bottomWallDistance = bot.ArenaHeight - bot.Y;
        if (bottomWallDistance < wallMargin)
            fy -= wallForceConstant / Math.Max(1, bottomWallDistance * bottomWallDistance);
        
        return (fx, fy);
    }

    // Calculate extra force to avoid corners
    public (double totalFx, double totalFy) CalculateCornerForce(double wallMargin, double cornerForceConstant)
    {
        double fx = 0, fy = 0;
        double cornerMargin = wallMargin * 1.5;
        
        // Check if near any corner
        bool nearTopLeft = bot.X < cornerMargin && bot.Y < cornerMargin;
        bool nearTopRight = bot.X > bot.ArenaWidth - cornerMargin && bot.Y < cornerMargin;
        bool nearBottomLeft = bot.X < cornerMargin && bot.Y > bot.ArenaHeight - cornerMargin;
        bool nearBottomRight = bot.X > bot.ArenaWidth - cornerMargin && bot.Y > bot.ArenaHeight - cornerMargin;
        
        if (nearTopLeft)
        {
            double distance = Math.Sqrt(bot.X * bot.X + bot.Y * bot.Y);
            fx += cornerForceConstant / Math.Max(1, distance * distance);
            fy += cornerForceConstant / Math.Max(1, distance * distance);
        }
        
        if (nearTopRight)
        {
            double dx = bot.ArenaWidth - bot.X;
            double distance = Math.Sqrt(dx * dx + bot.Y * bot.Y);
            fx -= cornerForceConstant / Math.Max(1, distance * distance);
            fy += cornerForceConstant / Math.Max(1, distance * distance);
        }
        
        if (nearBottomLeft)
        {
            double dy = bot.ArenaHeight - bot.Y;
            double distance = Math.Sqrt(bot.X * bot.X + dy * dy);
            fx += cornerForceConstant / Math.Max(1, distance * distance);
            fy -= cornerForceConstant / Math.Max(1, distance * distance);
        }
        
        if (nearBottomRight)
        {
            double dx = bot.ArenaWidth - bot.X;
            double dy = bot.ArenaHeight - bot.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            fx -= cornerForceConstant / Math.Max(1, distance * distance);
            fy -= cornerForceConstant / Math.Max(1, distance * distance);
        }
        
        return (fx, fy);
    }
}