using Godot;
using System;
using System.Collections.Generic;

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

    public void ToggleAttackHitbox(bool toggle, float damage, float knockbackAmount)
    {
        Tempo tempo = (Tempo)player;
        tempo.attackBox.damage = damage;
        tempo.attackBox.knockbackAmount = knockbackAmount;
        tempo.attackBox.SetDeferred("monitorable", toggle);
        tempo.attackBox.SetDeferred("monitoring", toggle);
    }

    public void ToggleAttackHitbox(float damage, float knockbackAmount, int index)
    {
        Tempo tempo = (Tempo)player;
        tempo.attackBox.damage = damage;
        tempo.attackBox.SetDeferred("monitoring", true);
        Godot.Collections.Array<Node> shapes = tempo.attackBox.GetChildren();
        for (int i = 0; i < shapes.Count; i++)
        {
            CollisionShape2D cs = (CollisionShape2D)shapes[i];
            if (i == index) cs.SetDeferred("disabled", false);
            else cs.SetDeferred("disabled", true);
        }
    }

    public enum AttackBoxes
    {
        GroundAttack,
        AerialAttack
    }

    public bool AttackCollisionProcess(float currentAngle)
    {
        Tempo tempo = (Tempo)player;
        CollisionShape2D cs = (CollisionShape2D)tempo.attackBox.GetChild((int)AttackBoxes.GroundAttack);
        float angle = Mathf.DegToRad(currentAngle);
        cs.Rotation = -angle;
        if ((currentAngle >= 316f && currentAngle <= 360f) || (currentAngle >= 0f && currentAngle <= 44f))
        {
            sensorTable["E"].Position = new Vector2(-player.pushRadius, 0);
            sensorTable["E"].direction = "left";
            sensorTable["F"].Position = new Vector2(player.pushRadius, 0);
            sensorTable["F"].direction = "right";
            if (player.playerSprite.FlipH) cs.Position = new Vector2(Mathf.Cos(angle) * -tempo.attackRadius, Mathf.Sin(angle) * tempo.attackRadius);
            else cs.Position = new Vector2(Mathf.Cos(angle) * tempo.attackRadius, Mathf.Sin(angle) * -tempo.attackRadius);;
            return false;
        }
        else if (currentAngle >= 45f && currentAngle <= 135f)
        {
            sensorTable["E"].Position = new Vector2(0, player.pushRadius);
            sensorTable["E"].direction = "down";
            sensorTable["F"].Position = new Vector2(0, -player.pushRadius);
            sensorTable["F"].direction = "up";
            if (player.playerSprite.FlipH) cs.Position = new Vector2(Mathf.Cos(angle) * -tempo.attackRadius, Mathf.Sin(angle) * tempo.attackRadius);
            else cs.Position = new Vector2(Mathf.Cos(angle) * tempo.attackRadius, Mathf.Sin(angle) * -tempo.attackRadius);;
            return true;
        }
        else if (currentAngle >= 136f && currentAngle <= 224f)
        {
            sensorTable["E"].Position = new Vector2(player.pushRadius, 0);
            sensorTable["E"].direction = "right";
            sensorTable["F"].Position = new Vector2(-player.pushRadius, 0);
            sensorTable["F"].direction = "left";
            if (player.playerSprite.FlipH) cs.Position = new Vector2(Mathf.Cos(angle) * tempo.attackRadius, Mathf.Sin(angle) * -tempo.attackRadius);
            else cs.Position = new Vector2(Mathf.Cos(angle) * -tempo.attackRadius, Mathf.Sin(angle) * tempo.attackRadius);;
            return false;
        }
        else 
        {
            sensorTable["E"].Position = new Vector2(0, -player.pushRadius);
            sensorTable["E"].direction = "up";
            sensorTable["F"].Position = new Vector2(0, player.pushRadius);
            sensorTable["F"].direction = "down";
            if (player.playerSprite.FlipH) cs.Position = new Vector2(Mathf.Cos(angle) * -tempo.attackRadius, Mathf.Sin(angle) * tempo.attackRadius);
            else cs.Position = new Vector2(Mathf.Cos(angle) * tempo.attackRadius, Mathf.Sin(angle) * -tempo.attackRadius);;
            return true;
        }
    }

    //ground collision that ignores angle, used to snap to ground disregarding slope
    public bool GroundCollisionSnapProcess()
    {
        bool groundCollision = true;
        SolidTileData groundData = GroundSensorCompetition();

        //if player is too far off the ground, they won't collide. this formula
        //takes player speed into account. the faster they move, the further they can
        //be off the ground and still collide
        if (sensorTable["A"].direction == "right" || sensorTable["A"].direction == "left")
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.ySpeed) + 4, 14))
                groundCollision = false;
        }
        else 
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.xSpeed) + 4, 14))
                groundCollision = false;
        }
        if (groundCollision)
        {
            if (sensorTable["A"].direction == "right")
                player.Position = new Vector2(player.GlobalPosition.X + groundData.distance - 1,  player.GlobalPosition.Y); 
            else if (sensorTable["A"].direction == "down")
                player.Position = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y + groundData.distance); 
            else if (sensorTable["A"].direction == "up")
                player.Position = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y - groundData.distance);
            else 
                player.Position = new Vector2(player.GlobalPosition.X - groundData.distance + 1,  player.GlobalPosition.Y);
        }
        else 
        {
            SwitchGroundCollisionMode(0);
            SwitchPushCollisionMode(0);
            player.psm.TransitionState(new PlayerFall());
        }
        return groundCollision;
    }

    public override bool CheckIfGrounded()
    {
        bool groundCollision = true;
        SolidTileData groundData = GroundSensorCompetition();

        //if player is too far off the ground, they won't collide. this formula
        //takes player speed into account. the faster they move, the further they can
        //be off the ground and still collide
        if (sensorTable["A"].direction == "right" || sensorTable["A"].direction == "left")
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.ySpeed) + 4, 14))
                groundCollision = false;
        }
        else 
        {
            if (groundData.distance > Mathf.Min(Mathf.Abs(player.xSpeed) + 4, 14))
                groundCollision = false;
        }
        return groundCollision;
    }
}
