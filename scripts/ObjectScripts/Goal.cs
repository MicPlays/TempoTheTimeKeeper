using Godot;
using System;

public partial class Goal : GameObject
{
    public float exitTimer;
    bool timerCountDown = false;
    public override void _Ready()
    {
        hitbox = GetNode<Hitbox>(hitboxPath);
        hitbox.AreaEntered += OnAreaEnter;
    }

    public void OnAreaEnter(Area2D area)
    {
        if (area is Hitbox)
        {
            Hitbox hb = (Hitbox)area;
            if (hb.parentObject is Player)
            {
                if (!(LevelManager.Instance.GetLevel().player.psm.CurrentState is PlayerLevelEnd))
                {
                    //exitTimer = 300f * (float)GetPhysicsProcessDeltaTime();
                    //timerCountDown = true;
                    LevelManager.Instance.GetLevel().timerActive = false;
                    //LevelManager.Instance.SaveTime();
                    LevelManager.Instance.GetLevel().player.psm.TransitionState(new PlayerLevelEnd());
                    if (LevelManager.Instance.GetLevel().activeCamera is PlayerCam)
                        ((PlayerCam)LevelManager.Instance.GetLevel().activeCamera).cameraLocked = true;
                }
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        
    }

}
