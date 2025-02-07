using Godot;
using System;

public partial class Spikes : GameObject
{
    public enum Facing
    {
        Left,
        Right,
        Up,
        Down
    }
    [Export]
    public Facing facing {get; set;}
    public override void _Ready()
    {
        hitbox = GetNode<Area2D>(hitboxPath);
        hitbox.AreaEntered += OnAreaEnter;
    }

    public void OnAreaEnter(Area2D area)
    {
        
        if (area is Hitbox)
        {
            Hitbox attackable = (Hitbox)area;
            if (attackable.parentObject is IAttackable)
                ((IAttackable)attackable.parentObject).Damage();
        }
        else 
        {
            Player player = GameController.Instance.GetPlayer();
            player.psm.TransitionState(new PlayerHurt());
            /*
            switch (facing)
            {
                case Facing.Left:
                    if (player.xSpeed < 0)
                        player.psm.TransitionState(new PlayerHurt());
                    break;
                case Facing.Right:
                    if (player.xSpeed > 0)
                        player.psm.TransitionState(new PlayerHurt());
                    break;
                case Facing.Up:
                    if (player.ySpeed >= 0 && (GlobalPosition.Y - player.GlobalPosition.Y) > 0)
                        player.psm.TransitionState(new PlayerHurt());
                    break;
                case Facing.Down:
                    if (player.ySpeed < 0)
                        player.psm.TransitionState(new PlayerHurt());
                    break;
            }
            */
            
        }
    }
}
