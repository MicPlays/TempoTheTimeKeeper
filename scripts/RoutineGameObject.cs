using Godot;
using System;

public abstract partial class RoutineGameObject : GameObject
{
    public int routineCount;

    public abstract void IncrementRoutine();
}
