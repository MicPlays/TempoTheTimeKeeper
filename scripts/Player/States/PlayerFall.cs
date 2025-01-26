using Godot;
using System;

public partial class PlayerFall : State
{
    public override void Enter(StateMachine sm)
    {
        
    }

    public override void Run(StateMachine sm, double delta)
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

            if ((player.playerSprite.Animation == "airtime" || player.playerSprite.Animation == "liftoff") && airAngle >= 180f && (airAngle < 360f))
                player.playerSprite.Play("airtransition");
                
            player.controlLockTimer = Mathf.Clamp(player.controlLockTimer - deltaTime, 0, 30 * deltaTime);
        }
    }
    public override void Exit(StateMachine sm)
    {
        
    }
}
