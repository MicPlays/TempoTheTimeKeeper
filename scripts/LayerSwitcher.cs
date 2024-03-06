using Godot;
using System;

public partial class LayerSwitcher : Node2D
{
    //toggle to only switch layer if player is grounded
    [Export]
    public bool groundedOnly;
    //flag to store the side of the layer switcher the player is on (true = left/up, false = right/down)
    [Export]
    public bool currentSide;
    //toggle to only switch player's visual layer
    [Export]
    public bool priorityOnly;

    //which direction the switcher is oriented in(true for vertical, false for horizontal)
    private bool orientation;

    [Export]
    public int widthRadius = 1;
    [Export]
    public int heightRadius = 10;

    //set which side of the switcher corresponds to which collision layer
    //0 is layer 1, 1 is layer 2. 
    [Export]
    public int sideA;
    [Export]
    public int sideB;

    public Player player;

    public override void _Ready()
    { 
        player = (Player)GetNode("/root/DebugRoot/Player");
        if (widthRadius > heightRadius)
            orientation = false;
        else if (widthRadius < heightRadius)
            orientation = true;
        //width and height radius should not be equal
        else orientation = true;
    }

    public override void _Process(double delta)
    {
        //if vertical orientation
        if (orientation == true)
        {
            //if player is in range
            
            if (Position.Y - heightRadius <= player.Position.Y && player.Position.Y <= Position.Y + heightRadius)
            {
                //if currentSide is left
                if (currentSide == true)
                {
                    if (player.Position.X <= Position.X)
                    {
                        player.currentLayer = sideB;
                        if (!priorityOnly) player.visualLayer = sideB;
                        currentSide = false;
                    }
                }
                //currentSide is right
                else
                {
                    if (player.Position.X >= Position.X)
                    {
                        player.currentLayer = sideA;
                        if (!priorityOnly) player.visualLayer = sideA;
                        currentSide = true;
                    }
                }
            }
        }
        //if horizontal orientation
        else 
        {
            //if player is in range
            if (Position.X - widthRadius <= player.Position.X && player.Position.X <= Position.X + widthRadius)
            {
                //if currentSide is up
                if (currentSide == true)
                {
                    if (player.Position.Y >= Position.Y)
                    {
                        player.currentLayer = sideB;
                        if (!priorityOnly) player.visualLayer = sideB;
                        currentSide = false;
                    }
                }
                //currentSide is down
                else
                {
                    if (player.Position.Y <= Position.Y)
                    {
                        player.currentLayer = sideA;
                        if (!priorityOnly) player.visualLayer = sideA;
                        currentSide = true;
                    }
                }
            }
        }
    }
}
