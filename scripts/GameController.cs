using Godot;
using System;

public partial class GameController : Node2D
{
    [Export]
    public NodePath playerPath;
    private static Player playerRef;
    private static bool timerActive;
    private static double timeSec;
    private static int minutes;
    private static int score;

    public static GameController Instance {get; private set;}

    public override void _Ready()
    {
        Instance = this;
        playerRef = GetNode<Player>(playerPath);
        timerActive = true;
    }

    public Player GetPlayer()
    {
        return playerRef;
    }

    public override void _Process(double delta)
    {
        if (timerActive)
        {
            timeSec += delta;
            double hundSec = timeSec % 1;
            double seconds = timeSec - hundSec;
            if (timeSec > 60)
            {
                minutes += 1;
                timeSec = 0;
            }
            HUD.Instance.SetTimer(minutes, seconds, hundSec);
        }  
    }

    public void AddScore(int scoreAdd)
    {
        score += scoreAdd;
        HUD.Instance.SetScore(score);
    }
}
