using Godot;
using System;

public partial class Sensor : Node2D
{
    [Export]
    public string direction;
    private TileMap tileMap;

    //for debug drawing of sensors
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

    //will seperate into another function that is called by parent object. will return data for collision
    public int[] CheckForTile()
    {
        mode = 0;
        Vector2I currentGridCell = new Vector2I((int)GlobalPosition.X/16, (int)GlobalPosition.Y/16);
        int[] distanceData = GetHeight(currentGridCell);
        int index = distanceData[0];
        int detectedHeight = distanceData[1];

        //extension (if detected tile is empty, extend out by one tile in sensor's direction)
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
            int[] newDistanceData = GetHeight(newGridCell);
            int newIndex = newDistanceData[0];
            int newDetectedHeight = newDistanceData[1];

            //if second tile is not empty, return that tile's data. else return the distance to the end of the second tile
            if (newDetectedHeight >= 1) 
            {
                QueueRedraw();
                int[] data = new int[3];
                data[0] = GetDistance(newIndex, newDetectedHeight, newGridCell);
                data[1] = (int)tileMap.GetCellTileData(-1, newGridCell).GetCustomData("angle");
                data[2] = tileMap.GetCellSourceId(-1, newGridCell);
                return data;
            }
            else
            {
                QueueRedraw();
                int[] data = new int[3];
                data[0] = GetDistance(0, 0, newGridCell);
                data[1] = -1;
                data[2] = -1;
                return data;
            }
        }
        //regression (if detected tile is a full block, pull back one tile in sensor's direction)
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
            int[] newDistanceData = GetHeight(newGridCell);
            int newIndex = newDistanceData[0];
            int newDetectedHeight = newDistanceData[1];

            //if second tile is not empty, return that tile's collision data. else, original tile
            //must be the ground so return original tile's data
            if (newDetectedHeight >= 1) 
            {
                QueueRedraw();
                int[] data = new int[3];
                data[0] = GetDistance(newIndex, newDetectedHeight, newGridCell);
                data[1] = (int)tileMap.GetCellTileData(-1, newGridCell).GetCustomData("angle");
                data[2] = tileMap.GetCellSourceId(-1, newGridCell);
                return data;
            }
            else
            {
                mode = 0;
                QueueRedraw();
                int[] data = new int[3];
                data[0] = GetDistance(index, detectedHeight, currentGridCell);
                data[1] = (int)tileMap.GetCellTileData(-1, currentGridCell).GetCustomData("angle");
                data[2] = tileMap.GetCellSourceId(-1, currentGridCell);
                return data;
            }
        }
        //normal case
        else 
        {
            mode = 0;
            QueueRedraw();
            int[] data = new int[3];
            data[0] = GetDistance(index, detectedHeight, currentGridCell);
            data[1] = (int)tileMap.GetCellTileData(-1, currentGridCell).GetCustomData("angle");
            data[2] = tileMap.GetCellSourceId(-1, currentGridCell);  
            return data;
        }
    }

    //calculate the detected height of a tile using either its width or height array.
    //returns both the detected height and the index used in the calculation.
    //returns zeroes if tile is undefined or tile cannot be detected by the given sensor.
    private int[] GetHeight(Vector2I gridSquare)
    {
        TileData tileData = tileMap.GetCellTileData(-1, gridSquare);
        if (tileData == null) return new int[] {0, 0};
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
            return new int[] {index, tileArray[index]};
        }
        else return new int[] {0, 0};
    }
    
    //calculate distance between tile surface and sensor location
    public int GetDistance(int index, int detectedHeight, Vector2I gridCell)
    {
        Vector2I tileSurface = new Vector2I(0, 0);

            if (direction == "right")
            {
                tileSurface.Y = (gridCell.Y * 16) + index + 1;
                tileSurface.X = ((gridCell.X * 16) + 16) - detectedHeight;
                return (int)(tileSurface.X - GlobalPosition.X);
            }
            else if (direction == "left")
            {
                tileSurface.Y = (gridCell.Y * 16) + index + 1;
                tileSurface.X = (gridCell.X * 16) + detectedHeight;
                return (int)(GlobalPosition.X - tileSurface.X);
            }
            else
            {
                tileSurface.X = (gridCell.X * 16) + index + 1;
                tileSurface.Y = ((gridCell.Y * 16) + 16) - detectedHeight;
                return (int)(tileSurface.Y - GlobalPosition.Y);
            }
    }
    public override void _Draw()
    {
        //normal draw
        //if (mode == 0)
        //{
            if (direction == "left" || direction == "right") 
                DrawLine(Vector2.Zero, new Vector2(-Position.X, 0), Colors.Green, 1);
            else
                DrawLine(Vector2.Zero, new Vector2(0, -Position.Y), Colors.Green, 1);
        //}
        /*
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
        */
    }


}
