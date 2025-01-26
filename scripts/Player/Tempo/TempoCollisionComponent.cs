using Godot;
using System;

public partial class TempoCollisionComponent : PlayerCollisionComponent
{
    public override void AirPushCollisionProcess()
    {
        Tempo tempo = (Tempo)player;
        Sensor activeSensor;
        //get active sensor
        if (player.xSpeed > 0)
            activeSensor = sensorTable["F"];
        else 
            activeSensor = sensorTable["E"];
        
        SolidTileData pushData = activeSensor.CheckForTile(player.currentLayer);
        if (pushData.distance <= 0)
        {
            //reset player position (push against wall)
            if (player.xSpeed > 0)
                player.Position = new Vector2(player.Position.X + pushData.distance, player.Position.Y);
            else if (player.xSpeed < 0)
                player.Position = new Vector2(player.Position.X - pushData.distance, player.Position.Y);
            tempo.xSpeedBuffer = player.xSpeed;
            player.xSpeed = 0;
            player.psm.TransitionState(new TempoWallJump());
        }
    }

    public bool AirLeftWallCollisionCheck()
    {
        SolidTileData pushData = sensorTable["E"].CheckForTile(player.currentLayer);
        if (pushData.distance <= 0)
            return true;
        else return false;
    }

    public bool AirRightWallCollisionCheck()
    {
        SolidTileData pushData = sensorTable["F"].CheckForTile(player.currentLayer);
        if (pushData.distance <= 0)
            return true;
        else return false;
    }

    public override bool DetermineAirCollisionMode(float airAngle)
    {
        bool landed = base.DetermineAirCollisionMode(airAngle);
        if (landed) player.psm.TransitionState(new TempoGrounded());
        return landed;
    }
}
