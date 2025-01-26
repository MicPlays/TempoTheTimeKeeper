using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameController : Node2D
{
    [Export]
    public NodePath playerPath;
    [Export]
    public NodePath layerSwitcherContainerPath;
    private static Tempo playerRef;
    private static bool timerActive;
    private static double timeSec;
    private static int minutes;
    private static int score;

    public static GameController Instance {get; private set;}

    public override void _Ready()
    {
        Instance = this;
        playerRef = GetNode<Tempo>(playerPath);
        timerActive = true;
        var layerSwitchNodes = GetNode<Node2D>(layerSwitcherContainerPath).GetChildren();
        foreach (var node in layerSwitchNodes)
        {
            LayerSwitcher layerSwitcher = (LayerSwitcher)node;
            layerSwitcher.player = playerRef;
        }

    }

    public Tempo GetPlayer()
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
