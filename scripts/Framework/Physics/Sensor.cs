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
        tileMap = LevelManager.Instance.GetLevel().tm;
    }

    //will seperate into another function that is called by parent object. will return data for collision
    public SolidTileData CheckForTile(int layer)
    {
        mode = 0;
        Vector2I currentGridCell = new Vector2I((int)GlobalPosition.X/16, (int)GlobalPosition.Y/16);
        int[] distanceData = GetHeight(currentGridCell, layer);
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
            int[] newDistanceData = GetHeight(newGridCell, layer);
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
                float angle = GetAngle(layer, newGridCell, hFlip, vFlip);
                bool flagged = (bool)tileMap.GetCellTileData(layer, newGridCell).GetCustomData("flagged");
                return new SolidTileData(distance, angle, flagged);
            }
            else
            {
                float distance = GetDistance(0, 0, newGridCell, false, false);
                float angle = 0f;
                bool flagged = false;
                return new SolidTileData(distance + (16 - distance), angle, flagged);
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
            int[] newDistanceData = GetHeight(newGridCell, layer);
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
                float angle = GetAngle(layer, newGridCell, hFlip, vFlip);
                bool flagged = (bool)tileMap.GetCellTileData(layer, newGridCell).GetCustomData("flagged");
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
                float angle = GetAngle(layer, currentGridCell, hFlip, vFlip);
                bool flagged = (bool)tileMap.GetCellTileData(layer, currentGridCell).GetCustomData("flagged");
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
            float angle = GetAngle(layer, currentGridCell, hFlip, vFlip);
            bool flagged = (bool)tileMap.GetCellTileData(layer, currentGridCell).GetCustomData("flagged");
            return new SolidTileData(distance, angle, flagged);
        }
    }

    //calculate the detected height of a tile using either its width or height array.
    //returns the detected height, the index used in the calculation, and the hFlip and vFlip properties of the tile.
    //returns zeroes if tile is undefined or tile cannot be detected by the given sensor.
    private int[] GetHeight(Vector2I gridSquare, int layer)
    {
        TileData tileData = tileMap.GetCellTileData(layer, gridSquare);
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
            int tileID = tileMap.GetCellAlternativeTile(layer, gridSquare);

            int tileSourceID = tileMap.GetCellSourceId(layer, gridSquare);
            TileSetAtlasSource atlas = (TileSetAtlasSource)tileMap.TileSet.GetSource(tileSourceID);
            Vector2I tileAtlasCoords = tileMap.GetCellAtlasCoords(layer, gridSquare);
            int tileNum = tileAtlasCoords.X + tileAtlasCoords.Y * 16;
            if (direction == "left" || direction == "right")
                tileArray = (int[])LevelManager.Instance.GetLevel().collisionData[tileNum].wArray.Clone(); 
            else 
            {
                tileArray = (int[])LevelManager.Instance.GetLevel().collisionData[tileNum].hArray.Clone();
                isHeight = true;
            }
            int index;
            Vector2 tilePos = new Vector2(gridSquare.X * 16, gridSquare.Y * 16);
            int hFlip = -1;
            int vFlip = -1;
            //based on flip property, flip array to get correct collision data
            if (tileID == 4096)
            {
                hFlip = 1;
                if (isHeight) Array.Reverse(tileArray);
            }     
            if (tileID == 8192)
            {
                vFlip = 1;
                if (!isHeight) Array.Reverse(tileArray);   
            }
            if (tileID == 12288)
            {
                hFlip = 1;
                if (isHeight) Array.Reverse(tileArray);
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

    //get angle of tile, if tile is an alternative tile, calculates angle from source tile angle
    public float GetAngle(int layer, Vector2I gridSquare, bool hFlip, bool vFlip)
    {
        int tileSourceID = tileMap.GetCellSourceId(layer, gridSquare);
        TileSetAtlasSource atlas = (TileSetAtlasSource)tileMap.TileSet.GetSource(tileSourceID);
        Vector2I tileAtlasCoords = tileMap.GetCellAtlasCoords(layer, gridSquare);
        int tileNum = tileAtlasCoords.X + tileAtlasCoords.Y * 16;
        float angle = LevelManager.Instance.GetLevel().collisionData[tileNum].angle; 
        if (hFlip && vFlip)
            return angle + 180;
        else if (hFlip)
            return 360f - angle;
        else if (vFlip) 
            return 180 - angle;
        else return angle;
}

}
