using Godot;
using System;

public partial class Rolante : CharacterBody2D
{
    GameManager manager;
    Player player;	

    [Export] int rayCastDistanceInTiles = 10;
    [Export] byte totalLife = 4;
    [Export] byte damage = 1;
    [Export] float rotationSpeed = 50f;
    [Export] float speed = 1000;
    [Export] float timeToAct = 2f;
    [Export] float StunnedTime = 1f;
    [Export] byte coinsToDrop = 1;

    RayCast2D rayCast;
    RayCast2D inFrontCast;
    VisibleOnScreenNotifier2D visibleNotifier;
    Area2D hitArea;

    Timer timerToAct;
    Timer stunnedTimer;

    LifeSystem lifeSystem;

    Vector2 playerPos;

    bool followLastPlayerPos = false;
    bool isStunned = false;

    public override void _Ready()
    {
        player = Player.Instance;
        manager = GameManager.Instance;

        hitArea = GetNode<Area2D>("HitArea");
        hitArea.BodyEntered += WhenHit;

        lifeSystem = new(totalLife, totalLife);
        lifeSystem.WhenDies = Die;

        visibleNotifier = NodeMisc.GenVisibleNotifier(this);

        rayCast = new()
        {
            CollisionMask = hitArea.CollisionMask,
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };
        AddChild(rayCast);

        inFrontCast = new()
        {
            CollisionMask = hitArea.CollisionMask,
            TargetPosition = new Vector2(2 * GameManager.GAMEUNITS, 0)
        };
        AddChild(inFrontCast);

        timerToAct = NodeMisc.GenTimer(this, timeToAct, WhenAct);
        stunnedTimer = NodeMisc.GenTimer(this, StunnedTime, StopStun);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isStunned) return;
        if (followLastPlayerPos)
        {
            if (inFrontCast.IsColliding() && inFrontCast.GetCollider() != player && inFrontCast.GetCollider() != null)
            {
                Stun();
                return;
            }
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

    void Stun() {
        if (!followLastPlayerPos) return;
        isStunned = true;
        stunnedTimer.Start();
        hitArea.GetChild<CollisionShape2D>(0).CallDeferred("set_disabled", true);
        timerToAct.Stop();
    }

    public void StopStun()
    {
        isStunned = false;
        hitArea.GetChild<CollisionShape2D>(0).CallDeferred("set_disabled", false);

        followLastPlayerPos = false;
    }

    public void Damage(float modifier, int amount = 1)
    {
        lifeSystem.GetDamage(modifier, amount);
    }

    void WhenAct()
    {
        playerPos = player.GlobalPosition;
        followLastPlayerPos = true;
        inFrontCast.LookAt(playerPos);
    }

    void Die()
    {
        manager.SpawnCoins(GlobalPosition, coinsToDrop);
        QueueFree();
    }

    void WhenHit(Node2D body)
    {
        if(isStunned) return;
        if (body is Player p) p.Damage(damage);
        else
        {
            Stun();
        }
    }
}
