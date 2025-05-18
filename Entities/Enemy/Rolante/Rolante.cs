using Godot;
using System;

public partial class Rolante : CharacterBody2D
{
    [Export] Player player;	
    [Export] int rayCastDistanceInTiles = 10;
    [Export] byte totalLife = 5;
    [Export] byte damage = 1;
    [Export] float rotationSpeed = 50f;
    [Export] float speed = 1500;
    [Export] float timeToAct = 1.5f;
    RayCast2D rayCast;
    VisibleOnScreenNotifier2D visibleNotifier;
    Area2D hitArea;

    Timer timerToAct;

    LifeSystem lifeSystem;

    Vector2 playerPos;

    bool followLastPlayerPos = false;
    float targetAngle = 0;

    public override void _Ready()
    {
        hitArea = GetNode<Area2D>("HitArea");
        hitArea.BodyEntered += WhenHit;
        lifeSystem = new(totalLife, totalLife);
        visibleNotifier = NodeMisc.GenVisibleNotifier(this);

        rayCast = new()
        {
            CollisionMask = hitArea.CollisionMask,
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };
        AddChild(rayCast);

        timerToAct = NodeMisc.GenTimer(this, timeToAct, WhenAct);
    }

    public override void _Process(double delta)
    {
        if (Rotation != targetAngle)
        {
            Rotation = Mathf.LerpAngle(Rotation, targetAngle, (float)delta * rotationSpeed);
        }
    }

    public override void _PhysicsProcess(double delta)
    {

        if (followLastPlayerPos)
        {
            Velocity = (playerPos - GlobalPosition).Normalized() * speed * GameManager.GAMEUNITS * (float)delta;
            MoveAndSlide();
            if (MathM.IsInRange(GlobalPosition, playerPos, 5f))
            {
                followLastPlayerPos = false;
            }
        }
        else
        {
            rayCast.LookAt(player.GlobalPosition);
            if (rayCast.IsColliding() && rayCast.GetCollider() == player)
            {
                if (timerToAct.IsStopped())
                    timerToAct.Start();
            }
            else
            {
                if (!timerToAct.IsStopped())
                    timerToAct.Stop();
            }
        }

    }


    void Damage(float modifier, int amount)
    {
        lifeSystem.GetDamage(modifier, amount);
    }

    void WhenAct()
    {
        playerPos = player.GlobalPosition;
        followLastPlayerPos = true;
        targetAngle = (playerPos - GlobalPosition).Angle();

    }

    void WhenHit(Node2D body)
    {
        if(body is Player p) p.Damage(damage);
    }
}
