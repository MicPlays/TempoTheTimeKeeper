using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node
{
    
    public static LevelManager Instance { get; private set; }
    private int currentLevelIndex = 0;
    private GameScene currentGameScene;


    public override void _Ready()
    {
        Instance = this;
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
                break;
            }
        }
    }

    public void MenuLoad()
    {

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

    public void SwapScene(int sceneIndex)
    {
        //write any data that needs to be saved at this point
        //check if index corresponds to menu or level, do respective load
    }
}
