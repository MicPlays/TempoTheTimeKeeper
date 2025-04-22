using Godot;
using System;

public partial class TempoAttackCombo : BaseState
{
    public bool transitioning;
    public float transitionTimer = 20f;
    public int comboStep = 1;
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            comboStep = 1;
            tcc.ToggleAttackHitbox(40, (int)TempoCollisionComponent.AttackBoxes.GroundAttack);
            player.playerSprite.AnimationFinished += AttackFinished;
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            float deltaTime = (float)delta;
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;

            //wall collision
            bool isVertical = tcc.AttackCollisionProcess(player.groundAngle);
            float wallDistance = player.cc.PushCollisionProcess(isVertical);
            wallDistance *= 60 * deltaTime;

            player.pc.SetSpeedOnGround(isVertical, wallDistance);
            tpc.MovePlayerObject();

            if (transitioning)
            {
                if (transitionTimer <= 0)
                {
                    player.SetState((int)PlayerStates.Grounded);
                }
                else 
                {
                    if (Input.IsActionPressed("left"))
                    {
                        player.pc.LeftGroundForce(deltaTime);
                        player.playerSprite.FlipH = true;
                    }    
                    else if (Input.IsActionPressed("right"))
                    {
                        player.pc.RightGroundForce(deltaTime);
                        player.playerSprite.FlipH = false;
                    }
                    else player.pc.Decelerate(deltaTime);
                        
                    if (Input.IsActionJustPressed("attack"))
                    {
                        transitioning = false;
                        comboStep++;
                        if (comboStep >= player.maxAttackCombo)
                        {
                            tcc.ToggleAttackHitbox(true, 60);
                            tpc.ApplyAttackForce(deltaTime);
                        }
                        else tcc.ToggleAttackHitbox(true, 40);
                    }
                }
                transitionTimer = Mathf.Clamp(transitionTimer - deltaTime, 0, player.comboTimerMax * deltaTime);
            }
            else 
            {
                player.playerSprite.Play("attackcombo" + comboStep);
            }

            bool groundCollision = player.cc.GroundCollisionProcess();
            if (groundCollision)
            {
                if (Mathf.Abs(-player.groundAngle - player.playerSprite.RotationDegrees) > 20f) 
                    player.playerSprite.RotationDegrees = -player.groundAngle;
            }
            else
            {
                player.playerSprite.Play("fall");
                player.playerSprite.SpeedScale = 1.0f;
                return;
            }
            if (player.controlLockTimer == 0)
            {
                bool falling = player.pc.CheckForSlip(deltaTime);
                if (falling)
                {
                    player.cc.SwitchGroundCollisionMode(0);
                    player.cc.SwitchPushCollisionMode(0);
                    player.playerSprite.Play("airtransition");
                    player.playerSprite.SpeedScale = 1.0f;
                    player.currentFrame = 0;
                    return;
                }
            }
            else player.controlLockTimer = Mathf.Clamp(player.controlLockTimer - deltaTime, 0, 30 * deltaTime);
        }
    }

    public override void Exit(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            comboStep = 1;
            player.playerSprite.AnimationFinished -= AttackFinished;
            tcc.ToggleAttackHitbox(false, 0);
        }
    }

    public void AttackFinished()
    {
        transitioning = true;
        PlayerStateMachine psm = LevelManager.Instance.GetLevel().player.psm;
        Tempo player = (Tempo)psm.player;
        TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
        tcc.ToggleAttackHitbox(false, 0);
        if (comboStep >= player.maxAttackCombo)
            comboStep = 0;
        transitionTimer = player.comboTimerMax * (float)player.GetPhysicsProcessDeltaTime();
    }
}
