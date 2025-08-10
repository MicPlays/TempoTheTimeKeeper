using Godot;
using System;

public partial class HUD : Control
{
    [Export]
    public NodePath minutesPath;
    [Export]
    public NodePath secondsPath;
    [Export]
    public NodePath hundPath;
    [Export]
    public NodePath noteCountPath;
    [Export]
    public NodePath scorePath;
    [Export]
    public NodePath healthPath;
    [Export]
    public NodePath transitionPath;

    private static Label minutesText;
    private static Label secondsText;
    private static Label hundSecText;
    private static Label noteCountText;
    private static Label scoreText;
    private static HealthUIManager healthContainer;
    private static ColorRect transition;

    public override void _Ready()
    {
        minutesText = GetNode<Label>(minutesPath);
        secondsText = GetNode<Label>(secondsPath);
        hundSecText = GetNode<Label>(hundPath);
        noteCountText = GetNode<Label>(noteCountPath);
        scoreText = GetNode<Label>(scorePath);
        healthContainer = GetNode<HealthUIManager>(healthPath);
        transition = GetNode<ColorRect>(transitionPath);

        BuildHealthBar();
    }

    public void SetTimer(int minutes, double timeSec, double hundSec)
    {
        string hundString = hundSec.ToString();
        if (hundString.Length < 4)
            hundString = "0" + hundString.Substring(2, 1);
        else hundString = hundString.Substring(2, 2);

        string secString = timeSec.ToString();
        if (secString.Length == 1)
            secString = "0" + secString;

        string minuteString = minutes.ToString();
        if (minuteString.Length == 1)
            minuteString = "0" + minuteString;

        minutesText.Text = minuteString;
        secondsText.Text = secString;
        hundSecText.Text = hundString;
    }

    public void SetScore(int score)
    {
        string scoreString = score.ToString();
        for (int i = scoreString.Length; i < 9; i++)
        {
            scoreString = "0" + scoreString;
        }
        scoreText.Text = scoreString;
    }

    public void SetNoteCount(int noteCount)
    {
        noteCountText.Text = noteCount.ToString();
    }

    public void BuildHealthBar()
    {
        if (healthContainer != null)
            healthContainer.Init(LevelManager.Instance.GetLevel().player.maxHealth);
        else GD.Print("error loading health UI");
    }

    public void SetHealth(int index)
    {
        healthContainer.SwapSprite(index);
    }

    public void SetTransitionAlpha(float alphaValue)
    {
        if (alphaValue > 1) alphaValue = 1;
        transition.Color = new Color(0, 0, 0, alphaValue);
    }

    public float GetTransitionAlpha()
    {
        return transition.Color.A;
    }
}
