using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class Level : GameScene
{
    public Player player;
    public PlayerCam playerCam;
    public CameraHolder activeCamera;
    public HUD hud;

    private static bool timerActive;
    private static double timeSec;
    private static int minutes;
    private static int score;

    [Export]
    public string collisionDataPath = "";

    [Export]
    public Node2D playerSpawnPoint;

    [Export]
    public NodePath layerSwitcherContainer;

    public Dictionary<int, List<int[]>> collisionData = new Dictionary<int, List<int[]>>();
    public TileMap tm;

    public override void Init()
    {
        collisionDataPath = ProjectSettings.GlobalizePath(collisionDataPath);
        LoadCollisionData();
        GD.Print("col data loaded");
        GD.Print(collisionData[1][0]);
        var tms = GetTree().GetNodesInGroup("tilemap");
        tm = (TileMap)tms[0];

        var playerScene = GD.Load<PackedScene>(PackedSceneConstants.Player);
        player = (Player)playerScene.Instantiate();
        AddChild(player);
        player.Position = playerSpawnPoint.Position;

        GD.Print("player spawned");

        var hudScene = GD.Load<PackedScene>(PackedSceneConstants.HUD);
        hud = (HUD)hudScene.Instantiate();
        GetNode("CanvasLayer").AddChild(hud);
        
        var cameraScene = GD.Load<PackedScene>(PackedSceneConstants.PlayerCamera);
        
        //for now is always the player cam, might change if make more cameras
        activeCamera = (CameraHolder)cameraScene.Instantiate();
        AddChild(activeCamera);
        if (activeCamera is PlayerCam) ((PlayerCam)activeCamera).target = player;
        Camera2D cam = GetCameraFromNode(activeCamera);
        cam.LimitLeft = 0;

        //set up layer switchers
        var layerSwitchNodes = GetNode<Node2D>(layerSwitcherContainer).GetChildren();
        foreach (var node in layerSwitchNodes)
        {
            LayerSwitcher layerSwitcher = (LayerSwitcher)node;
            layerSwitcher.player = player;
        }
        timerActive = true;
    }

    public void LoadCollisionData()
    {
        if (!File.Exists(collisionDataPath)) return;
        using (StreamReader sr = File.OpenText(collisionDataPath))
        {
            int lineCounter = 0;
            int tileKey = 0;
            string s;
            List<int[]> cArrs = new List<int[]>();
            while ((s = sr.ReadLine()) != null)
            {
                if (lineCounter == 0)
                {
                    tileKey = Int32.Parse(s);
                    lineCounter++;
                }
                else
                {
                    
                    string[] strings = s.Split(":");
                    string dataStr = strings[1];
                    string[] data = dataStr.Split(",");
                    int[] cArr = new int[data.Length];
                    for (int i = 0; i < data.Length; i++)
                        cArr[i] = Int32.Parse(data[i]);
                    cArrs.Add(cArr);
                    lineCounter++;
                    if (lineCounter > 2)
                    {
                        List<int[]> arrs = new List<int[]>();
                        arrs.Add(cArrs[0]);
                        arrs.Add(cArrs[1]);
                        collisionData.Add(tileKey, arrs);
                        cArrs.Clear();
                        lineCounter = 0;
                    }
                }
            }
        }
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
            hud.SetTimer(minutes, seconds, hundSec);
        }  
    }

    public void AddScore(int scoreAdd)
    {
        score += scoreAdd;
        hud.SetScore(score);
    }

    public static Camera2D GetCameraFromNode(Node node)
    {
        if (node is CameraHolder)
        {
            CameraHolder ch = (CameraHolder)node;
            return ch.GetCamera();
        }   
        else if (node is Camera2D)
            return (Camera2D)node;
        else return null;
    }
}
