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
    [Export] float speed = 350;
    [Export] float timeToAct = 2f;
    [Export] float StunnedTime = 1f;
    [Export] byte coinsToDrop = 1;
    [Export] ItemResource itemThatMightDrop = null;
    [Export] byte itemDropPercentage = 50;

    RayCast2D rayCast;
    RayCast2D inFrontCast;
    VisibleOnScreenNotifier2D visibleNotifier;
    Area2D hitArea;
    AudioHandler audioHandler;

    Timer timerToAct;
    Timer stunnedTimer;

    LifeSystem lifeSystem;
    AnimationHandler animationHandler;
    Sprite2D sp;

    Vector2 playerPos;

    bool followLastPlayerPos = false;
    bool isStunned = false;
    
    Vector2 SpawnPosition;
    Timer respawnTimer;
    bool isDead = false;

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
        sp = GetNode<Sprite2D>("Sprite");

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


        SpawnPosition = GlobalPosition;
        respawnTimer = NodeMisc.GenTimer(this, GameManager.RESPAWNTIME, () =>
        {
            isDead = false;
            GlobalPosition = SpawnPosition;
        });
    }

    void Activate()
    {
        if(isDead) return;
        
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
        audioHandler.Stop();
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
            Vector2 dir = (playerPos - GlobalPosition).Normalized();
            if(dir.X > 0) sp.FlipH = false;
            else sp.FlipH = true;

            Velocity = dir * speed * GameManager.GAMEUNITS * (float)delta;

            MoveAndSlide();

            if (MathM.IsInRange(GlobalPosition, playerPos, 10f))
            {
                followLastPlayerPos = false;
                animationHandler.Play("back");
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
        animationHandler.Play("stun");
    }

    public void StopStun()
    {
        isStunned = false;
        hitArea.GetChild<CollisionShape2D>(0).CallDeferred("set_disabled", false);

        followLastPlayerPos = false;
        sp.FrameCoords = new(2, 0);
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
        animationHandler.Play("walk");
    }

    void Die()
    {
        animationHandler.Die();
        Timer toDie = NodeMisc.GenTimer(this, (float)animationHandler.GetAnimationTime(), () =>
        {
            if (itemThatMightDrop != null)
            {
                byte chance = (byte)GameManager.rnd.Next(1, 100);
                if(chance >= itemDropPercentage)
                    manager.SpawnItem(GlobalPosition, itemThatMightDrop, 1 );
            }
            manager.SpawnCoins(GlobalPosition, coinsToDrop);
            // QueueFree();
            GlobalPosition = GameManager.deadPosition;
            isDead = true;
            respawnTimer.Start();
            DeActivate();
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
