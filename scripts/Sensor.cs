using Godot;
using Godot.NativeInterop;
using System;

public partial class Sensor : Node2D
{
    [Export]
    public string direction;
    private TileMap tileMap;

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
        Vector2I currentGridCell = new Vector2I((int)GlobalPosition.X/16, (int)GlobalPosition.Y/16);
        TileData tileData = tileMap.GetCellTileData(-1, currentGridCell);
        if (tileData != null)
        {
            //Get height/width array of tile if sensor can detect that tile
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
                if ("direction" == "left" || direction == "right") 
                    tileArray = (int[])tileData.GetCustomData("width_array");
                else 
                    tileArray = (int[])tileData.GetCustomData("height_array");
                
                //get index of array to use in collision calculation
                Vector2 tilePos = tileMap.MapToLocal(currentGridCell);
                int index;
                if ("direction" == "left" || direction == "right") 
                    index = (int)(GlobalPosition.Y - tilePos.Y);
                else
                    index = (int)(GlobalPosition.X - tilePos.X);

                
            }       
        } 
    }

    public override void _Draw()
    {
        if (direction == "left" || direction == "right") 
            DrawLine(Vector2.Zero, new Vector2(-Position.X, 0), Colors.Green, 1);
        else
            DrawLine(Vector2.Zero, new Vector2(0, -Position.Y), Colors.Green, 1);
    }
}
