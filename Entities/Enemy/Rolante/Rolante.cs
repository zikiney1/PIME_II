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
    [Export] float speed = 700;
    [Export] float timeToAct = 2f;
    [Export] float StunnedTime = 1f;
    [Export] byte coinsToDrop = 1;

    RayCast2D rayCast;
    RayCast2D inFrontCast;
    VisibleOnScreenNotifier2D visibleNotifier;
    Area2D hitArea;
    AudioHandler audioHandler;

    Timer timerToAct;
    Timer stunnedTimer;

    LifeSystem lifeSystem;
    AnimationHandler animationHandler;

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

        animationHandler = new(GetNode<AnimationPlayer>("Animation/AnimationPlayer"), GetNode<AnimationPlayer>("Animation/HitAnimationPlayer"));
        audioHandler = GetNode<AudioHandler>("AudioHandler");


        visibleNotifier = new();
        visibleNotifier.ScreenEntered += Activate;
        visibleNotifier.ScreenExited += DeActivate;

        rayCast = new()
        {
            CollisionMask = hitArea.CollisionMask,
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };

        inFrontCast = new()
        {
            CollisionMask = hitArea.CollisionMask,
            TargetPosition = new Vector2(2 * GameManager.GAMEUNITS, 0)
        };

        
        AddChild(rayCast);
        AddChild(inFrontCast);
        AddChild(visibleNotifier);

        timerToAct = NodeMisc.GenTimer(this, timeToAct, WhenAct);
        stunnedTimer = NodeMisc.GenTimer(this, StunnedTime, StopStun);
        DeActivate();
    }

    void Activate()
    {
        SetPhysicsProcess(true);
        SetProcess(true);
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        hitArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        GetNode<Sprite2D>("Sprite").Visible = true;
        hitArea.Monitoring = true;
        hitArea.Monitorable = true;
    }
    void DeActivate()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        GetNode<Sprite2D>("Sprite").Visible = false;
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        hitArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        hitArea.Monitoring = false;
        hitArea.Monitorable = false;
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

    void Stun()
    {
        if (!followLastPlayerPos) return;
        isStunned = true;
        stunnedTimer.Start();
        hitArea.GetChild<CollisionShape2D>(0).CallDeferred("set_disabled", true);
        timerToAct.Stop();
        audioHandler.PlaySpecialSFX(0);
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
        animationHandler.Damage();
        audioHandler.PlayHit();
    }

    void WhenAct()
    {
        playerPos = player.GlobalPosition;
        followLastPlayerPos = true;
        inFrontCast.LookAt(playerPos);
        audioHandler.PlayShoot();
    }

    void Die()
    {
        animationHandler.Die();
        Timer toDie = NodeMisc.GenTimer(this, (float)animationHandler.GetAnimationTime(), () =>
        {
            manager.SpawnCoins(GlobalPosition, coinsToDrop);
            QueueFree();

        });
        toDie.Start();
        audioHandler.PlayDie();
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
