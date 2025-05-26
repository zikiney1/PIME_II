using Godot;
using System;

public partial class Projectile : Area2D
{
    CollisionShape2D collision;
    Sprite2D sprite;
    VisibleOnScreenNotifier2D visibleNotifier;
    public Vector2 direction = Vector2.Zero;
    public float speed = 200f;
    public Pooling pooling;
    public Action<Node2D> WhenBodyEnter;

    public override void _Ready()
    {
        collision = new();
        sprite = new();


        visibleNotifier = new();
        visibleNotifier.ScreenExited += DeSpawn;

        BodyEntered += (body) =>
        {
            if (!Visible) return;
            WhenBodyEnter?.Invoke(body);
            DeSpawn();
        };

        AddChild(sprite);
        AddChild(collision);
        AddChild(visibleNotifier);

        DeActivate();
    }


    public override void _Process(double delta)
    {
        if(direction == Vector2.Zero) return;

        GlobalPosition += speed * direction * (float)delta;
    }

    public void SetTexture(Texture2D texture){
        sprite.Texture = texture;
        collision.Shape = new RectangleShape2D(){
            Size = new Vector2(sprite.Texture.GetWidth(),sprite.Texture.GetHeight())
        };
    }

    public void Activate(){
        Visible = true;
        SetProcess(true);
        SetPhysicsProcess(true);
    }
    public void DeActivate(){
        Visible = false;
        SetProcess(false);
        SetPhysicsProcess(false);
    }


    public void DeSpawn()
    {
        pooling.ReturnBullet(this);
        DeActivate();
    }
}
