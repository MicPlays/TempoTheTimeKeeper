using System;
using Godot;


public abstract partial class BaseStateMachine : Node2D
{
	public BaseState CurrentState { get; private set; }
	public abstract void TransitionState(BaseState transitionState);

	public void SetState(BaseState state)
	{
		CurrentState = state;
	}


}