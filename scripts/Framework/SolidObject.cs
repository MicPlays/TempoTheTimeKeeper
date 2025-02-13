using Godot;
using System;

public partial class SolidObject : GameObject
{
    public int combinedXRadius;
    public int combinedYRadius; 
    [Export]
    public NodePath screenNotifierPath;
    public VisibleOnScreenNotifier2D screenNotifier;

    public override void _Ready()
    {
        screenNotifier = GetNode<VisibleOnScreenNotifier2D>(screenNotifierPath);
        screenNotifier.ScreenEntered += EnableObject;
        screenNotifier.ScreenExited += DisableObject;
    }

    public override void _PhysicsProcess(double delta)
    {
        float deltaTime = (float)delta;
        Player player = GameController.Instance.GetPlayer(); 

        if (player.standingOnObject)
        {
            float xLeftDistance = (player.GlobalPosition.X - GlobalPosition.X) + combinedXRadius;
            if ((xLeftDistance < 0) || xLeftDistance >= (combinedXRadius * 2))
            {
                player.standingOnObject = false;
                player.SetState(1);
            }
        }
        //horizontal overlap
        float leftDifference = (player.GlobalPosition.X - GlobalPosition.X) + combinedXRadius;
        if ((leftDifference < 0) || (leftDifference > combinedXRadius * 2))
            return;
        //vertical overlap
        float topDifference = (player.GlobalPosition.Y - GlobalPosition.Y) + 4 + combinedYRadius;
        if ((topDifference < 0) || (topDifference > combinedYRadius * 2))
            return;
        
        float xDistance;
        float yDistance;
        //right edge distance
        if (player.GlobalPosition.X > GlobalPosition.X)
            xDistance = leftDifference - combinedXRadius * 2;
        //left edge distance
        else xDistance = leftDifference;
        //bottom edge distance
        if (player.GlobalPosition.Y > GlobalPosition.Y)
            yDistance = topDifference - 4 - (combinedYRadius * 2);
        //top edge distance
        else yDistance = topDifference;

        if ((Mathf.Abs(xDistance) > Mathf.Abs(yDistance)) || (Mathf.Abs(yDistance) <= 4))
        {
            //vertical collision
            if (yDistance < 0)
            {
                if (player.ySpeed == 0)
                {
                   if (player.cc.CheckIfGrounded())
                    player.SetState((int)PlayerStates.Death);
                }
                else if (player.ySpeed < 0)
                {
                    player.GlobalPosition = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y - yDistance);
                    player.ySpeed = 0;
                }
            }
            else
            {
                if (!(yDistance >= 16))
                {
                    yDistance -= 4;
                    float xComparison = GlobalPosition.X - player.GlobalPosition.X + combinedXRadius;
                    if (xComparison < 0) return;
                    if (xComparison >= combinedXRadius * 2) return;
                    if (player.ySpeed < 0) return;
                    player.GlobalPosition = new Vector2(player.GlobalPosition.X, player.GlobalPosition.Y - yDistance - 1);
                    player.ySpeed = 0;
                    player.groundAngle = 0;
                    player.standingOnObject = true;
                    player.SetState((int)PlayerStates.Grounded);
                    player.groundSpeed = player.xSpeed;
                }
            }
        }
        else
        {
            //horizontal collision
            if (xDistance != 0)
            {
                if (player.xSpeed < 0 && xDistance < 0)
                {
                    player.xSpeed = 0;
                    player.groundSpeed = 0;
                }
                else if (player.xSpeed > 0 && xDistance > 0)
                {
                    player.xSpeed = 0;
                    player.groundSpeed = 0;
                }
            }
            player.GlobalPosition = new Vector2(player.GlobalPosition.X - xDistance, player.GlobalPosition.Y);
        }
    }

    public void CalcCombinedRadius()
    {
        Player player = GameController.Instance.GetPlayer(); 
        combinedXRadius = widthRadius + player.pushRadius + 1;
        combinedYRadius = heightRadius + player.heightRadius;
    }

    public virtual void EnableObject()
    {
        CalcCombinedRadius();
        //QueueRedraw();
        SetPhysicsProcess(true);
    }

    public virtual void DisableObject()
    {
        SetPhysicsProcess(false);
    }

    public override void _Draw()
    {
        /*
        Rect2 rect = new Rect2(-combinedXRadius, -combinedYRadius, combinedXRadius * 2, combinedYRadius * 2);
        GD.Print(rect);
        DrawRect(rect, new Color(Colors.Orange));
        */
    }
}
