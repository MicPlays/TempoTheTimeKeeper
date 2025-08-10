using Godot;
using System;

public partial class PlayerDeath : BaseState
{
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            player.pc.SetDeathSpeedAndDirection((float)player.GetPhysicsProcessDeltaTime());
            if (LevelManager.Instance.GetLevel().activeCamera is PlayerCam)
                ((PlayerCam)LevelManager.Instance.GetLevel().activeCamera).cameraLocked = true;
            player.hitbox.SetDeferred("monitorable", false);
            player.deathTimer = player.deathTransitionTime;
        }
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            float deltaTime = (float)delta;
            player.playerSprite.Play("death");
            player.pc.MovePlayerObject();
            player.pc.ApplyGravity(deltaTime);
            if (!player.screenNotifer.IsOnScreen())
            {
                float alpha = LevelManager.Instance.GetLevel().hud.GetTransitionAlpha();
                GD.Print(alpha);
                LevelManager.Instance.GetLevel().hud.SetTransitionAlpha(alpha + 0.05f);
                LevelManager.Instance.GetLevel().timerActive = false;
                player.deathTimer -= deltaTime * 60;
                if (player.deathTimer <= 0) LevelManager.Instance.ReloadCurrentLevel();
            }
            
        }
    }

    public override void Exit(BaseStateMachine sm)
    {

    }
}
