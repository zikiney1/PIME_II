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
    public Pooling pooling;

    public override void _Ready()
    {
        collision = new();
        sprite = new();
        visibleNotifier = new();

        AddChild(sprite);
        AddChild(collision);
        AddChild(visibleNotifier);

        visibleNotifier.ScreenExited += () => {this.DeSpawn();};
        BodyEntered += (body)=>{this.WhenEnterBody(body);};

        DeActivate();
    }


    public override void _Process(double delta)
    {
        base._Process(delta);
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


    public virtual void WhenEnterBody(Node2D body){}

    public virtual void DeSpawn(){}
}
