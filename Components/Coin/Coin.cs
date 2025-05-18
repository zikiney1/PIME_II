using Godot;
using System;

public partial class Coin : Area2D
{
    Player player;
    public Pooling pool;
    public Vector2 startPosition;
    const float amplitude = 4;
    const float frequency = 6;
    float time = 0;
    public byte quantity = 0;
    Sprite2D sp;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        sp = new()
        {
            Texture = GD.Load<Texture2D>("res://assets/Sprites/test/coin.png")
        };
        CollisionShape2D cs = new()
        {
            Shape = new RectangleShape2D()
            {
                Size = new(sp.Texture.GetWidth() * 2, sp.Texture.GetHeight() * 2)
            }
        };
        player = Player.Instance;

        AddChild(sp);
        AddChild(cs);

        BodyEntered += (body) =>
        {
            if (body is Player p)
            {
                p.AddGold(quantity);
                pool.ReturnCoin(this);
            }
        };
    }
    public override void _Process(double delta)
    {
        if (time >= float.MaxValue) time = 0;
        time += (float)delta;
        float yOffset = Mathf.Sin(time * frequency) * amplitude;

        sp.Position = new Vector2(0, yOffset);

        if (MathM.IsInRange(player.GlobalPosition, GlobalPosition, GameManager.GAMEUNITS * 5))
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += direction * GameManager.GAMEUNITS * 2 * (float)delta;
        }
    }


    public void DeActivate(){
        Visible = false;
        SetPhysicsProcess(false);
        SetProcess(false);
    }
    
    public void Activate(){
        Visible = true;
        SetPhysicsProcess(true);
        SetProcess(true);
    }
}
