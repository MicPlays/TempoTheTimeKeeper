using Godot;
using System;

public partial class PlayerFall : BaseState
{
    public override void Enter(BaseStateMachine sm)
    {
        
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        float deltaTime = (float)delta;
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;

            //input movement
            if (Input.IsActionPressed("left"))
                player.pc.LeftAirForce(deltaTime);
            else if (Input.IsActionPressed("right"))
                player.pc.RightAirForce(deltaTime);

            player.pc.AirDrag(deltaTime);
            player.pc.MovePlayerObject();
            player.pc.ApplyGravity(deltaTime);
            player.pc.RotateGroundAngle();
            player.playerSprite.RotationDegrees = -player.groundAngle;
            float airAngle = player.cc.GetAngleOfAirMovement();
            
            player.cc.AirPushCollisionProcess();
            player.cc.DetermineAirCollisionMode(airAngle);

            if ((player.playerSprite.Animation == "airtime") && airAngle >= 180f && (airAngle < 360f))
                player.playerSprite.Play("airtransition");

            if (player.playerSprite.Animation == "liftoff" && !player.playerSprite.IsPlaying()) player.playerSprite.Play("airtime");

            if (!player.playerSprite.IsPlaying() && player.playerSprite.Animation == "airtransition") player.playerSprite.Play("fall");
                
            player.controlLockTimer = Mathf.Clamp(player.controlLockTimer - deltaTime, 0, 30 * deltaTime);
        }
    }
    public override void Exit(BaseStateMachine sm)
    {
        
    }
}
