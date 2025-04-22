using Godot;
using System;

public partial class TempoAerialAttack : PlayerFall
{
    public float attackTimer;
    public float spriteTimer;
    public override void Enter(BaseStateMachine sm)
    {
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            player.playerSprite.Play("airattack");
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            tcc.ToggleAttackHitbox(0, (int)TempoCollisionComponent.AttackBoxes.AerialAttack);
            attackTimer = player.airAttackMax;
            spriteTimer = player.airAttackSpriteTime;
        }
        
    }

    public override void Run(BaseStateMachine sm, double delta)
    {
        float deltaTime = (float)delta;
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Tempo player = (Tempo)psm.player;
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            attackTimer = Mathf.Clamp(attackTimer - deltaTime, 0, player.airAttackMax * deltaTime);
            spriteTimer = Mathf.Clamp(spriteTimer - deltaTime, 0, player.airAttackSpriteTime * deltaTime);
            if (attackTimer <= 0)
                tcc.ToggleAttackHitbox(false, 0);
            if (spriteTimer <= 0)
                psm.TransitionState(new PlayerFall());
            base.Run(sm, delta);
        }
    }

    public override void Exit(BaseStateMachine sm)
    {
        GD.Print("leave air attack");
        if (sm is PlayerStateMachine)
        {
            PlayerStateMachine psm = (PlayerStateMachine)sm;
            Player player = psm.player;
            player.playerSprite.Play("airattack");
            TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
            tcc.ToggleAttackHitbox(false, 0);
            player.playerSprite.Play("airtransition");
        }
    }
}
