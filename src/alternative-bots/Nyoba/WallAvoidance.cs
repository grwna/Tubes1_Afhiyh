using System;
using Robocode.TankRoyale.BotApi;

public class WallAvoidance
{
    private Bot bot;

    public WallAvoidance(Bot bot)
    {
        this.bot = bot;
    }

    // Check if bot is near any wall
    public bool IsNearWall(double margin)
    {
        return bot.X < margin || bot.Y < margin || 
               bot.X > bot.ArenaWidth - margin || bot.Y > bot.ArenaHeight - margin;
    }

    // Predict if moving in a direction will cause a wall collision
    public bool WillHitWall(double distance, double angle)
    {
        // Convert relative angle to absolute angle
        double absoluteAngle = bot.NormalizeAbsoluteAngle(bot.Direction + angle);
        double radAngle = absoluteAngle * Math.PI / 180;
        
        // Calculate future position
        double futureX = bot.X + distance * Math.Cos(radAngle);
        double futureY = bot.Y + distance * Math.Sin(radAngle);
        
        // Check if future position is outside arena bounds (with margin)
        double margin = 20;
        return futureX < margin || futureX > bot.ArenaWidth - margin ||
               futureY < margin || futureY > bot.ArenaHeight - margin;
    }

    // Apply wall smoothing by adjusting angle to avoid walls
    public void ApplyWallSmoothing(ref double angle)
    {
        // Find closest wall
        double leftDist = bot.X;
        double rightDist = bot.ArenaWidth - bot.X;
        double topDist = bot.Y;
        double bottomDist = bot.ArenaHeight - bot.Y;
        
        // Determine closest wall
        double minDist = Math.Min(Math.Min(leftDist, rightDist), Math.Min(topDist, bottomDist));
        
        // Adjust angle based on closest wall
        if (minDist == leftDist)
        {
            // If near left wall, avoid turning left
            if (angle > 90 && angle < 270)
            {
                angle = (angle > 180) ? 270 : 90;
            }
        }
        else if (minDist == rightDist)
        {
            // If near right wall, avoid turning right
            if (angle < 90 || angle > 270)
            {
                angle = (angle > 180) ? 180 : 0;
            }
        }
        else if (minDist == topDist)
        {
            // If near top wall, avoid turning up
            if (angle > 0 && angle < 180)
            {
                angle = (angle > 90) ? 180 : 0;
            }
        }
        else if (minDist == bottomDist)
        {
            // If near bottom wall, avoid turning down
            if (angle > 180 && angle < 360)
            {
                angle = (angle > 270) ? 0 : 180;
            }
        }
    }

    // Find maximum safe distance in a direction
    public double FindMaxSafeDistance(double angle, double maxSearchDistance)
    {
        double minDist = 0;
        double maxDist = maxSearchDistance * 3; // Try up to 3x normal move distance
        double bestDist = 0;
        
        // Binary search for max safe distance
        while (maxDist - minDist > 5)
        {
            double midDist = (minDist + maxDist) / 2;
            
            if (WillHitWall(midDist, angle))
            {
                maxDist = midDist;
            }
            else
            {
                minDist = midDist;
                bestDist = midDist;
            }
        }
        
        return bestDist;
    }

    // Find best angle to avoid a wall
    public double FindBestAvoidanceAngle(double moveDistance)
    {
        // Try several angles and find the one that gives maximum safe distance
        double[] angles = new double[8];
        for (int i = 0; i < 8; i++)
        {
            angles[i] = i * 45; // 0, 45, 90, 135, 180, 225, 270, 315
        }
        
        double bestAngle = 0;
        double maxDistance = 0;
        
        foreach (double angle in angles)
        {
            double distance = FindMaxSafeDistance(angle, moveDistance);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                bestAngle = angle;
            }
        }
        
        return bestAngle;
    }
}