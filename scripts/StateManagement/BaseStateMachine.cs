using System;
using Godot;


public abstract partial class StateMachine : Node
{
	public State CurrentState { get; private set; }
	public abstract void TransitionState(State transitionState);

	public void SetState(State state)
	{
		CurrentState = state;
	}


}