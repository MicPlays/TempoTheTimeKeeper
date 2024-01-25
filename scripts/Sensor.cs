using Godot;
using Godot.NativeInterop;
using System;

public partial class Sensor : Node2D
{
    [Export]
    public string direction;
    private TileMap tileMap;

    //for debug
    private int mode = 0;

    public override void _Ready()
    {
        //get all tilemaps in scene. set working tilemap to the one that matches the layer player is on
        var tilemaps = GetTree().GetNodesInGroup("tilemaps");
        int mapCounter = 0;
        foreach (TileMap map in tilemaps)
        {
            if (mapCounter == 0)
            {
                int layer = (int)map.GetMeta("layer"); 
                if (layer == 0)
                { 
                    this.tileMap = map;
                    mapCounter++;
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        mode = 0;
        Vector2I currentGridCell = new Vector2I((int)GlobalPosition.X/16, (int)GlobalPosition.Y/16);
        int detectedHeight = GetHeight(currentGridCell);
        //extension
        if (detectedHeight == 0)
        {
            mode = 1;
            Vector2I newGridCell = Vector2I.Zero;
            switch (direction)
            {
                case "left":
                {
                    newGridCell = new Vector2I(currentGridCell.X - 1, currentGridCell.Y);
                    break;
                }
                case "right":
                {
                    newGridCell = new Vector2I(currentGridCell.X + 1, currentGridCell.Y);
                    break;
                }
                case "up":
                {
                    newGridCell = new Vector2I(currentGridCell.X, currentGridCell.Y - 1);
                    break;
                }
                case "down":
                {

                    newGridCell = new Vector2I(currentGridCell.X, currentGridCell.Y + 1);
                    break;
                }
            }
            int newDetectedHeight = GetHeight(newGridCell);
            if (newDetectedHeight > 1) 
            {
                QueueRedraw();
                //GD.Print(Name + " Extension returned:" + newDetectedHeight);
            }
            else
            {
                QueueRedraw();
                //GD.Print(Name + " Extension could not find tile");
            }
        }
        //regression
        else if (detectedHeight == 16)
        {
            mode = 2;
            Vector2I newGridCell = Vector2I.Zero;
            switch (direction)
            {
                case "left":
                {   
                    newGridCell = new Vector2I(currentGridCell.X + 1, currentGridCell.Y);
                    break;
                }
                case "right":
                {
                    newGridCell = new Vector2I(currentGridCell.X - 1, currentGridCell.Y);
                    break;
                }
                case "up":
                {
                    newGridCell = new Vector2I(currentGridCell.X, currentGridCell.Y + 1);
                    break;
                }
                case "down":
                {
                    newGridCell = new Vector2I(currentGridCell.X, currentGridCell.Y - 1);
                    break;
                }
            }
            int newDetectedHeight = GetHeight(newGridCell);
            if (newDetectedHeight > 1) 
            {
                QueueRedraw();
                //GD.Print(Name + " Regression returned:" + newDetectedHeight);  
            }
            else
            {
                mode = 0;
                QueueRedraw();
                //GD.Print(Name + " Regression Using 1st Surface:" + detectedHeight);
            }
        }
        //normal case
        else 
        {
            QueueRedraw();
            mode = 0;
            //GD.Print(Name + " Normal Case:" + detectedHeight);  
        }
    }

    private int GetHeight(Vector2I gridSquare)
    {
        TileData tileData = tileMap.GetCellTileData(-1, gridSquare);
        if (tileData == null) return 0;
        String detection = (String)tileData.GetCustomData("sensors");
        int[] tileArray;
        bool canDetect = false;
        switch (detection)
        {
            case "full":
            {
                canDetect = true;
                break;
            }
            case "top-sides":
            {
                if (direction != "down") canDetect = true;
                break;
            }
            case "bottom":
            {
                if (direction == "down") canDetect = true;
                break;
            }
        }
        if (canDetect)
        {
            if (direction == "left" || direction == "right") 
                tileArray = (int[])tileData.GetCustomData("width_array");
            else 
                tileArray = (int[])tileData.GetCustomData("height_array");
            
            //get index of array to use in collision calculation
            Vector2 tilePos = new Vector2(gridSquare.X * 16, gridSquare.Y * 16);
            int index;
            if (direction == "left" || direction == "right")
                index = (int)(GlobalPosition.Y - tilePos.Y);
            else
                index = (int)(GlobalPosition.X - tilePos.X);
            return tileArray[index];
        }
        else return 0;
    }

    public override void _Draw()
    {
        //normal draw
        if (mode == 0)
        {
            if (direction == "left" || direction == "right") 
                DrawLine(Vector2.Zero, new Vector2(-Position.X, 0), Colors.Green, 1);
            else
                DrawLine(Vector2.Zero, new Vector2(0, -Position.Y), Colors.Green, 1);
        }
        //extension draw
        else if (mode == 1)
        {
            switch (direction)
            {
                case "left":
                {
                    DrawLine(new Vector2(-16f, 0), new Vector2(-Position.X, 0), Colors.Green, 1);
                    break;
                }
                case "right":
                {
                    DrawLine(new Vector2(16f, 0), new Vector2(-Position.X, 0), Colors.Green, 1);
                    break;
                }
                case "up":
                {
                    DrawLine(new Vector2(0, -16f), new Vector2(0, -Position.Y), Colors.Green, 1);
                    break;
                }
                case "down":
                {
                    DrawLine(new Vector2(0, 16f), new Vector2(0, -Position.Y), Colors.Green, 1);
                    break;
                }
            }
        }
        //regression draw
        else 
        {
            switch (direction)
            {
                case "left":
                {
                    DrawLine(new Vector2(16f, 0), new Vector2(-Position.X, 0), Colors.Green, 1);
                    break;
                }
                case "right":
                {
                    DrawLine(new Vector2(-16f, 0), new Vector2(-Position.X, 0), Colors.Green, 1);
                    break;
                }
                case "up":
                {
                    DrawLine(new Vector2(0, 16f), new Vector2(0, -Position.Y), Colors.Green, 1);
                    break;
                }
                case "down":
                {
                    DrawLine(new Vector2(0, -16f), new Vector2(0, -Position.Y), Colors.Green, 1);
                    break;
                }
            }
        }
    }
}
