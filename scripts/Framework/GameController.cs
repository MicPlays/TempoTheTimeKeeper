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
    [Export]
    public bool drawHitboxes {get; set;}
    private static Player playerRef;
    private static bool timerActive;
    private static double timeSec;
    private static int minutes;
    private static int score;
    [Export]
    public NodePath playerCamPath;
    public Camera2D playerCam;

    public static GameController Instance {get; private set;}

    public override void _Ready()
    {
        Instance = this;
        playerRef = GetNode<Player>(playerPath);
        playerCam = GetNode<Camera2D>(playerCamPath);
        timerActive = true;
        HUD.Instance.BuildHealthBar(playerRef.maxHealth);
        var layerSwitchNodes = GetNode<Node2D>(layerSwitcherContainerPath).GetChildren();
        foreach (var node in layerSwitchNodes)
        {
            LayerSwitcher layerSwitcher = (LayerSwitcher)node;
            layerSwitcher.player = playerRef;
        }
    }

    public Player GetPlayer()
    {
        return playerRef;
    }

    public override void _PhysicsProcess(double delta)
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
