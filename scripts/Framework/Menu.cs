using Godot;
using System;

public partial class Menu : Node2D
{
    [Export]
    public NodePath controlPath;
    public Control menuControl;
    public HBoxContainer bestTime;
    public HBoxContainer recentTime;


    [Export]
    public string nextScenePath;
    public override void _Ready()
    {
        menuControl = GetNode<Control>(controlPath);
        recentTime = (HBoxContainer)menuControl.GetChild(0); 
        bestTime = (HBoxContainer)menuControl.GetChild(1);
        LoadTimes();
    }

    //for now im just throwing this menu shit in here for the demo
    public void LoadTimes()
    {
        string[] bestStrings = LevelManager.Instance.bestTime.Split(":");
        ((Label)bestTime.GetChild(1)).Text = bestStrings[0];
        ((Label)bestTime.GetChild(3)).Text = bestStrings[1];
        ((Label)bestTime.GetChild(5)).Text = bestStrings[2];
        
        string[] recentStrings = LevelManager.Instance.currentTime.Split(":");
        ((Label)recentTime.GetChild(1)).Text = recentStrings[0];
        ((Label)recentTime.GetChild(3)).Text = recentStrings[1];
        ((Label)recentTime.GetChild(5)).Text = recentStrings[2];
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("start"))
        {
            LevelManager.Instance.SwapScene(nextScenePath);
        }
    }
}
