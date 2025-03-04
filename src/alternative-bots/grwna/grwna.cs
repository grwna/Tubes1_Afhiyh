using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;\
using random

public class grwna : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args)
    {
        new grwna().Start();
    }

    grwna() : base(BotInfo.FromFile("grwna.json")) { }

    public override void Run()
    {
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.FromArgb(0xff,0xff,0xff);
        GunColor = Color.FromArgb(0xff,0x0,0x0);
        RadarColor = Color.FromArgb(0xff,0x0,0x0);
        ScanColor = Color.FromArgb(0x0, 0x00, 0x00);
        BulletColor = Color.FromArgb(0x0, 0xcb, 0xFF);


        while (IsRunning)
        {
            Forward(100); Back(100); Fire(1);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    /* Read the documentation for more events and methods */
}
