using Godot;
using System;
using System.Collections.Generic;

public partial class HealthUIManager : Node
{
    public List<TextureRect> sprites;
    private PackedScene healthObject;
    [Export]
    public string ActiveTexturePath;
    [Export]
    public string InactiveTexturePath;
    private Texture2D activeTexture;
    private Texture2D inactiveTexture;


    public void Init(int health)
    {
        sprites = new List<TextureRect>();
        //later load texture based on character (if new characters are added)
        activeTexture = (Texture2D)GD.Load(ActiveTexturePath);
        inactiveTexture = (Texture2D)GD.Load(InactiveTexturePath);
        for (int i = 0; i < health; i++)
        {
            TextureRect healthSprite = new TextureRect();
            healthSprite.Texture = activeTexture;
            healthSprite.ExpandMode = TextureRect.ExpandModeEnum.FitWidth;
            healthSprite.StretchMode = TextureRect.StretchModeEnum.KeepAspect;
            sprites.Add(healthSprite);
            AddChild(healthSprite);
        }
    }

    public void SwapSprite(int index)
    {
        if (sprites[index].Texture == activeTexture)
            sprites[index].Texture = inactiveTexture;
        else 
            sprites[index].Texture = activeTexture;
    }
}
