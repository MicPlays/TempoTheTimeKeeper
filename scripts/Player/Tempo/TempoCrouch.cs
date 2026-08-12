using Godot;
using System;

public partial class TempoCrouch : BaseState
{
	public override void Enter(BaseStateMachine sm)
    {
		if (sm is PlayerStateMachine)
		{
			PlayerStateMachine psm = (PlayerStateMachine)sm;
			Tempo player = (Tempo)psm.player;
			player.speedBoostInputTimer = 0;

			//begin play crouch anim

			//shrink hitbox size
			TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
			tcc.SetHitboxSize(new Vector2(tcc.hitboxX, tcc.hitboxY/2));
		}
}

    public override void Run(BaseStateMachine sm, double delta)
    {
		float deltaTime = (float)delta;
		PlayerStateMachine psm = (PlayerStateMachine)sm;
		Tempo player = (Tempo)psm.player;
		TempoPhysicsComponent tpc = (TempoPhysicsComponent)player.pc;
        TempoCollisionComponent tcc = (TempoCollisionComponent)player.cc;
		
    }

    public override void Exit(BaseStateMachine sm)
    {

    }

}
