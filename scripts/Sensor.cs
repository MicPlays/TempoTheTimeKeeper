using Godot;
using System;

public partial class Sensor : Node2D
{
    [Export]
    public string tileDetection;
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
    public SolidTileData CheckForTile()
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
                bool hFlip = false, vFlip = false;
                if (newDistanceData[2] > 0)
                    hFlip = true;
                if (newDistanceData[3] > 0)
                    vFlip = true;
                float distance = GetDistance(newIndex, newDetectedHeight, newGridCell, hFlip, vFlip);
                float angle = (float)tileMap.GetCellTileData(-1, newGridCell).GetCustomData("angle");
                bool flagged = (bool)tileMap.GetCellTileData(-1, newGridCell).GetCustomData("flagged");
                return new SolidTileData(distance, angle, flagged);
            }
            else
            {
                float distance = GetDistance(0, 0, newGridCell, false, false);
                float angle = 0f;
                bool flagged = false;
                return new SolidTileData(distance, angle, flagged);
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
                bool hFlip = false, vFlip = false;
                if (newDistanceData[2] > 0)
                    hFlip = true;
                if (newDistanceData[3] > 0)
                    vFlip = true;
                float distance = GetDistance(newIndex, newDetectedHeight, newGridCell, hFlip, vFlip);
                float angle = (float)tileMap.GetCellTileData(-1, newGridCell).GetCustomData("angle");
                bool flagged = (bool)tileMap.GetCellTileData(-1, newGridCell).GetCustomData("flagged");
                return new SolidTileData(distance, angle, flagged);
            }
            else
            {
                bool hFlip = false, vFlip = false;
                if (distanceData[2] > 0)
                    hFlip = true;
                if (distanceData[3] > 0)
                    vFlip = true;
                float[] data = new float[2];
                float distance = GetDistance(index, detectedHeight, currentGridCell, hFlip, vFlip);
                float angle = (float)tileMap.GetCellTileData(-1, currentGridCell).GetCustomData("angle");
                bool flagged = (bool)tileMap.GetCellTileData(-1, currentGridCell).GetCustomData("flagged");
                return new SolidTileData(distance, angle, flagged);
            }
        }
        //normal case
        else 
        {
            bool hFlip = false, vFlip = false;
            if (distanceData[2] > 0)
                hFlip = true;
            if (distanceData[3] > 0)
                vFlip = true;
            float distance = GetDistance(index, detectedHeight, currentGridCell, hFlip, vFlip);
            float angle = (float)tileMap.GetCellTileData(-1, currentGridCell).GetCustomData("angle");
            bool flagged = (bool)tileMap.GetCellTileData(-1, currentGridCell).GetCustomData("flagged");
            return new SolidTileData(distance, angle, flagged);
        }
    }

    //calculate the detected height of a tile using either its width or height array.
    //returns the detected height, the index used in the calculation, and the hFlip and vFlip properties of the tile.
    //returns zeroes if tile is undefined or tile cannot be detected by the given sensor.
    private int[] GetHeight(Vector2I gridSquare)
    {
        TileData tileData = tileMap.GetCellTileData(-1, gridSquare);
        if (tileData == null) return new int[] {0, 0, 0, 0};
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
                if (tileDetection != "bottom") canDetect = true;
                break;
            }
            case "bottom":
            {
                if (tileDetection == "bottom") canDetect = true;
                break;
            }
        }
        if (canDetect)
        {
            bool isHeight = false;
            int tileID = tileMap.GetCellAlternativeTile(-1, gridSquare);

            //if tile is an alternative tile, get the width or height array 
            //of the source tile that alt tile derives from
            if (tileID != 0)
            {
                //to get the data from the source tile, we need the TileSetAtlasSource to get the tile data from.
                //to get this TileSetAtlasSource, we need the source ID of the tile, retrieved from the tilemap.
                int tileSourceID = tileMap.GetCellSourceId(-1, gridSquare);
                TileSetAtlasSource atlas = (TileSetAtlasSource)tileMap.TileSet.GetSource(tileSourceID);
                //to get said tile data from the atlas source, we need the atlas coordinates, also retrieved from the tilemap.
                Vector2I tileAtlasCoords = tileMap.GetCellAtlasCoords(-1, gridSquare);
                TileData sourceTileData = atlas.GetTileData(tileAtlasCoords, 0);

                if (direction == "left" || direction == "right")
                    tileArray = (int[])sourceTileData.GetCustomData("width_array"); 
                else 
                {
                    tileArray = (int[])sourceTileData.GetCustomData("height_array");
                    isHeight = true;
                }
            }
            //otherwise just get height/width array
            else 
            {
                if (direction == "left" || direction == "right")
                    tileArray = (int[])tileData.GetCustomData("width_array"); 
                else 
                {
                    tileArray = (int[])tileData.GetCustomData("height_array");
                    isHeight = true;
                }
            }

            int index;
            Vector2 tilePos = new Vector2(gridSquare.X * 16, gridSquare.Y * 16);
            int hFlip = -1;
            int vFlip = -1;
            //based on flip property, flip array to get correct collision data
            if (tileData.FlipH)
            {
                hFlip = 1;
                if (isHeight) Array.Reverse(tileArray);
               
            }     
            if (tileData.FlipV)
            {
                vFlip = 1;
                if (!isHeight) Array.Reverse(tileArray);   
            }

            if (isHeight)  index = (int)(GlobalPosition.X - tilePos.X);
            else index = (int)(GlobalPosition.Y - tilePos.Y);

            return new int[] {index, tileArray[index], hFlip, vFlip};
        }
        else return new int[] {0, 0, 0, 0};
    }
    
    //calculate distance between tile surface and sensor location. assumes tiles are built from bottom to top (height) and
    //right to left (width) by default.
    public float GetDistance(int index, int detectedHeight, Vector2I gridCell, bool hFlip, bool vFlip)
    {
        Vector2I tileSurface = new Vector2I(0, 0);
        if (direction == "right")
        {
            tileSurface.Y = (gridCell.Y * 16) + index + 1;
            if (hFlip)
                tileSurface.X = ((gridCell.X * 16) + 16) - (16 - detectedHeight) - detectedHeight;
            else
                tileSurface.X = ((gridCell.X * 16) + 16) - detectedHeight;
            return tileSurface.X - GlobalPosition.X;
        }
        else if (direction == "left")
        {
            tileSurface.Y = (gridCell.Y * 16) + index + 1;
            if (hFlip)
                tileSurface.X = (gridCell.X * 16) + detectedHeight;
            else
                tileSurface.X = ((gridCell.X * 16) + 16) - (16 - detectedHeight);
            return GlobalPosition.X - tileSurface.X;
        }
        else if (direction == "up")
        {
            tileSurface.X = (gridCell.X * 16) + index + 1;
            if (vFlip)
                tileSurface.Y = (gridCell.Y * 16) + detectedHeight;
            else
                tileSurface.Y = (gridCell.Y * 16)  + (16 - detectedHeight) + detectedHeight;
            return GlobalPosition.Y - tileSurface.Y;
        }
        else 
        {
            tileSurface.X = (gridCell.X * 16) + index + 1;
            if (vFlip)
                tileSurface.Y = ((gridCell.Y * 16) + 16) - (16 - detectedHeight) - detectedHeight;
            else
                tileSurface.Y = ((gridCell.Y * 16) + 16) - detectedHeight;
            return tileSurface.Y - GlobalPosition.Y;
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
