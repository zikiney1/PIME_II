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

    Timer animationTimer;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Scale = new Vector2(0.25f, 0.25f);
        sp = new()
        {
            Texture = GD.Load<Texture2D>("res://assets/Sprites/test/coin.png")
        };
        sp.Hframes = 4;
        CollisionShape2D cs = new()
        {
            Shape = new RectangleShape2D()
            {
                Size = new((sp.Texture.GetWidth()/4), sp.Texture.GetHeight())
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

        animationTimer = NodeMisc.GenTimer(this, 0.2f, () =>
        {
            sp.Frame = (sp.Frame + 1) % 4;
            animationTimer.Start();
            GD.Print(sp.Frame);
        });
        
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


    public void DeActivate()
    {
        Visible = false;
        SetPhysicsProcess(false);
        SetProcess(false);
        animationTimer.Stop();
    }

    public void Activate()
    {
        Visible = true;
        SetPhysicsProcess(true);
        SetProcess(true);
        animationTimer.Start();
    }
}
