using System;
using Godot;

public class ReflectionHandler
{
    public Sprite2D ownSprite;
    public Sprite2D ReflectSprite;

    public ReflectionHandler(Sprite2D _ownSprite, Sprite2D _ReflectSprite)
    {
        this.ownSprite = _ownSprite;
        this.ReflectSprite = _ReflectSprite;
        ReflectSprite.Texture = ownSprite.Texture;
        ReflectSprite.Texture = ownSprite.Texture;
        ReflectSprite.Hframes = ownSprite.Hframes;
        ReflectSprite.Vframes = ownSprite.Vframes;
        // ReflectSprite.Rotation = -(float)Math.PI;
        // ReflectSprite.Position = new(0, (GameManager.GAMEUNITS * 1.5f));
        ReflectSprite.ZIndex = -14;
    }


    public void Update()
    {
        ReflectSprite.Frame = ownSprite.Frame;
    }
}