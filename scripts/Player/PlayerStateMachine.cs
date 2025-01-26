using Godot;
using System;

public partial class PlayerStateMachine : StateMachine
{
    public Player player;
    
    public override void TransitionState(State transitionState)
    {
        CurrentState.Exit(this);
        SetState(transitionState);
        CurrentState.Enter(this);
    }

    public override void _PhysicsProcess(double delta)
    {
        CurrentState.Run(this, delta);
    }
}
