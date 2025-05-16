using Godot;
using System;

public partial class Projectile : Area2D
{
    protected CollisionShape2D collision;
    protected Sprite2D sprite;
    public Vector2 direction = Vector2.Zero;
    public int damage = 1;
    public float speed = 200f;
    public VisibleOnScreenNotifier2D visibleNotifier;

    public override void _Ready()
    {
        collision = new();
        sprite = new();
        visibleNotifier = new();

        sprite.Texture = GD.Load<Texture2D>("res://icon.svg");
        ResizeCollision();


        AddChild(sprite);
        AddChild(collision);
        AddChild(visibleNotifier);

        visibleNotifier.ScreenExited += DeSpawn;
    }

    protected void ResizeCollision(){
        collision.Shape = new RectangleShape2D(){
            Size = new Vector2(sprite.Texture.GetWidth(),sprite.Texture.GetHeight())
        };

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(direction == Vector2.Zero) return;

        GlobalPosition += speed * direction * (float)delta;
    }

    public void Shoot(Vector2 direction){
        this.direction = direction;
    }


    public void DeSpawn(){
        QueueFree();
    }
}
