using Godot;

public abstract partial class State : Node
{
	public abstract void Enter(StateMachine gm);
	public abstract void Run(StateMachine gm, double delta);
	public abstract void Exit(StateMachine gm);
}
