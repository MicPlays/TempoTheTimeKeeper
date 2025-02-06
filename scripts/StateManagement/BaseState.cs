using Godot;

public abstract partial class BaseState : Node2D
{
	public abstract void Enter(BaseStateMachine gm);
	public abstract void Run(BaseStateMachine gm, double delta);
	public abstract void Exit(BaseStateMachine gm);
}

