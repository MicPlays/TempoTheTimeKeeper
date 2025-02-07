using Godot;
using System;

[Tool]
public partial class LayerSwitcher : Node2D
{
    enum Orientation
    { Vertical, Horizontal };
    enum Side
    { A, B };
    //toggle to only switch layer if player is grounded
    [Export]
    public bool groundedOnly;
    //toggle to only switch player's visual layer
    [Export]
    public bool priorityOnly;
    //set true if you want to draw switchers at runtime for debugging
    [Export]
    public bool drawDebug = false;

    //flag to store the side of the layer switcher the player is on (true = left/up, false = right/down)
    private Side currentSide;

    private Orientation orientation;

    private int _widthRadius = 1;
    private int _heightRadius = 1;

    [Export]
    public int WidthRadius
    {
        get => _widthRadius;
        set 
        {
            _widthRadius = value;
            if (WidthRadius > HeightRadius)
                orientation = Orientation.Horizontal;
            else if(WidthRadius < HeightRadius)
                orientation = Orientation.Vertical;
            UpdateConfigurationWarnings();
            QueueRedraw();
        }
        
    }

    [Export]
    public int HeightRadius
    {
        get => _heightRadius;
        set 
        {
            _heightRadius = value;
            if (WidthRadius > HeightRadius)
                orientation = Orientation.Horizontal;
            else if(WidthRadius < HeightRadius)
                orientation = Orientation.Vertical;
            UpdateConfigurationWarnings();
            QueueRedraw();
        }
        
    }

    //set which side of the switcher corresponds to which collision layer
    //0 is layer 1, 1 is layer 2. 
    [Export]
    public int sideA;
    [Export]
    public int sideB;
    [Export]
    public int visualA;
    [Export]
    public int visualB;

    public Player player;

    public override void _Ready()
    { 
        if (WidthRadius > HeightRadius)
            orientation = Orientation.Horizontal;
        else if(WidthRadius < HeightRadius)
            orientation = Orientation.Vertical;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Engine.IsEditorHint())
        {
            bool canSwitch = true;
            if (groundedOnly)
            {
                if (!(player.psm.CurrentState is PlayerGrounded))
                    canSwitch = false;
            }

            if (priorityOnly)
                canSwitch = false;
            
            //if player is in range
            if (
            !(Position.Y - HeightRadius <= player.Position.Y && player.Position.Y <= Position.Y + HeightRadius  &&
                Position.X - WidthRadius <= player.Position.X && player.Position.X <= Position.X + WidthRadius)
            )
            canSwitch = false;
            //if vertical orientation
            if (orientation == Orientation.Vertical)
            {
                //if currentSide is left
                if (currentSide == Side.A)
                {
                    if (player.Position.X >= Position.X)
                    {
                        if (canSwitch)
                            player.currentLayer = sideB;
                            
                        player.ZIndex = visualB;
                        currentSide = Side.B;
                    }
                }
                //currentSide is right
                else
                {
                    if (player.Position.X <= Position.X)
                    {
                        if (canSwitch)
                            player.currentLayer = sideA;

                        player.ZIndex= visualA;
                        currentSide = Side.A;
                    }
                }
            }
            //if horizontal orientation
            else 
            {
                //if currentSide is up
                if (currentSide == Side.A)
                {
                    if (player.Position.Y >= Position.Y)
                    {
                        if (canSwitch)
                            player.currentLayer = sideB;

                        player.ZIndex = visualB;
                        currentSide = Side.B;
                    }
                }
                //currentSide is down
                else
                {
                    if (player.Position.Y <= Position.Y)
                    {
                        if (canSwitch)
                            player.currentLayer = sideA;

                        player.ZIndex = visualA;
                        currentSide = Side.A;
                    }
                }
            }
        }
    }

    public override string[] _GetConfigurationWarnings()
    {  
        string[] warnings = {""};
        if (HeightRadius == WidthRadius)
        {
            warnings[0] = "Height and width radii cannot be equal";
            return warnings;
        }
        else return null;
    }

    public override void _Draw()
    {
        if (Engine.IsEditorHint() || drawDebug)
        {
            if (orientation == Orientation.Horizontal)
                DrawLine(new Vector2(-WidthRadius, 0), new Vector2(WidthRadius, 0), Colors.Orange, 1);
            else DrawLine(new Vector2(0, -HeightRadius), new Vector2(0, HeightRadius), Colors.Orange, 1);
        }
    }
}
