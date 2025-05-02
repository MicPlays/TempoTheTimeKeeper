using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class LevelManager : Node
{
    [Export]
    public string timeFilePath = "res://resources/besttimes.txt";
    public string bestTime;
    public string currentTime = "99:99:99";
    public static LevelManager Instance { get; private set; }
    private int currentLevelIndex = 0;
    private GameScene currentGameScene;


    public override void _Ready()
    {
        Instance = this;
        timeFilePath = ProjectSettings.GlobalizePath(timeFilePath);
        if (!File.Exists(timeFilePath)) bestTime = "99:99:99";
        else
        {
            using (StreamReader sr = File.OpenText(timeFilePath))
            {
                bestTime = sr.ReadLine();
            }
        }
        
        var sceneRoot = GetTree().Root.GetChildren();
        for (int i = 0; i < sceneRoot.Count; i++)
        {
            if (sceneRoot[i] is GameScene)
            {
                GameScene gs = (GameScene)sceneRoot[i];
                currentGameScene = gs;
                //if menu, do menu load operations
                if (gs is Level)
                {
                    LevelLoad((Level)gs);
                }
                else if (gs is Menu)
                {
                    MenuLoad((Menu)gs);
                } 
            }
        }
    }

    public void MenuLoad(Menu menu)
    {
        menu.Init();
    }

    public void LevelLoad(Level level)
    {
        currentLevelIndex = level.index;
        GD.Print("load level");
        level.Init();
    }

    public Level GetLevel()
    {
        if (currentGameScene is Level)
            return (Level)currentGameScene;
        else return null;
    }

    public void SwapScene(string scene)
    {
        GetTree().ChangeSceneToFile(scene);
        var sceneRoot = GetTree().Root.GetChildren();
        for (int i = 0; i < sceneRoot.Count; i++)
        {
            if (sceneRoot[i] is GameScene)
            {
                GameScene gs = (GameScene)sceneRoot[i];
                currentGameScene = gs;
                //if menu, do menu load operations
                if (gs is Level)
                {
                    LevelLoad((Level)gs);
                }
                else if (gs is Menu)
                {
                    MenuLoad((Menu)gs);
                } 
            }
        }
    }

    public string GetBestTime()
    {
        return bestTime;
    }

    public string GetCurrentTime()
    {
        return currentTime;
    }

    public void SaveTime()
    {
        double hundSec = GetLevel().timeSec % 1;
        double seconds = GetLevel().timeSec - hundSec;

        string hundString = hundSec.ToString();
        if (hundString.Length < 4)
            hundString = "0" + hundString.Substring(2, 1);
        else hundString = hundString.Substring(2, 2);
        
        string secString = seconds.ToString();
        if (secString.Length == 1)
            secString = "0" + secString;

        string minuteString = GetLevel().minutes.ToString();
        if (minuteString.Length == 1)
            minuteString = "0" + minuteString;

        string time = minuteString + ":" + secString + ":" + hundString; 

        string[] bestStrings = bestTime.Split(":");
        bool canSave = false;
        if (Int32.Parse(minuteString) < Int32.Parse(bestStrings[0]))
            canSave = true;
        else if (Int32.Parse(minuteString) == Int32.Parse(bestStrings[0]))
        {
            if (Int32.Parse(secString) < Int32.Parse(bestStrings[1])) 
                canSave = true;
            else if (Int32.Parse(secString) == Int32.Parse(bestStrings[1]))
            {
                if (Int32.Parse(hundString) < Int32.Parse(bestStrings[2]))
                canSave = true;
            }
        }
        if (canSave)
        {
            bestTime = time;
            if (!File.Exists(timeFilePath)) File.Create(timeFilePath);
            using (StreamWriter sr = new StreamWriter(timeFilePath))
            {
                sr.WriteLine(time);
            }
        }
        currentTime = time;
    }
}
