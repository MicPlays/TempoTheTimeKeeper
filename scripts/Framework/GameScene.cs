using Godot;
using System;

public abstract partial class GameScene : Node
{
    [Export]
    public int index;
    public abstract void Init();
    
}
