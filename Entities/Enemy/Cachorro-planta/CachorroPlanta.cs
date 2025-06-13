using Godot;
using System;

public partial class CachorroPlanta : CharacterBody2D
{
    [Export] public float attackSpeed = 0.5f;
    [Export] public byte totalLife = 2;
    [Export] public byte coinsToDrop = 5;
    [Export] public int distanceToPlayer = 20;
    [Export] public float Speed = 220;
    [Export] public byte damage = 1;
    [Export] public ItemResource itemThatMightDrop = null;
    [Export] public byte itemDropPercentage = 50;

    LifeSystem lifeSystem;
    FightSystem fightSystem;
    AnimationHandler animationHandler;
    AudioHandler audioHandler;

    Player player;
    GameManager manager;

    VisibleOnScreenNotifier2D visibleNotifier;
    NavigationAgent2D navAgent;

    RayCast2D VisionCast;
    Timer NavUpdateTimer;
    Area2D HitArea;

    bool isInVision => VisionCast.IsColliding() && VisionCast.GetCollider() == player;
    bool isGoingToDie = false;


    Vector2 spawnPosition;
    bool isDead = false;
    Timer respawnTimer;
    public override void _Ready()
    {
        base._Ready();
        NavUpdateTimer = NodeMisc.GenTimer(this, 0.5f, NavUpdate);

        player = Player.Instance;
        manager = GameManager.Instance;

        fightSystem = new(this, attackSpeed);
        lifeSystem = new(totalLife, totalLife);
        lifeSystem.WhenDies += Die;
        animationHandler = new(GetNode<AnimationPlayer>("Animation/AnimationPlayer"), GetNode<AnimationPlayer>("Animation/HitAnimationPlayer"));
        audioHandler = GetNode<AudioHandler>("AudioHandler");

        VisionCast = new()
        {
            CollisionMask = this.CollisionMask,
            TargetPosition = new Vector2(distanceToPlayer * GameManager.GAMEUNITS, 0)
        };
        AddChild(VisionCast);

        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        HitArea = GetNode<Area2D>("HitArea");
        HitArea.BodyEntered += WhenHit;

        visibleNotifier = new();
        AddChild(visibleNotifier);
        visibleNotifier.ScreenEntered += Activate;
        DeActivate();

        spawnPosition = GlobalPosition;
        respawnTimer = NodeMisc.GenTimer(this, GameManager.RESPAWNTIME, () =>
        {
            isDead = false;
            GlobalPosition = spawnPosition;
        });
    }

    void Activate()
    {
        if (isDead) return;
        NavUpdate();
        SetPhysicsProcess(true);
        SetProcess(true);
        HitArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        GetNode<Sprite2D>("Sprite").Visible = true;
        
    }
    void DeActivate()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        GetNode<Sprite2D>("Sprite").Visible = false;
        HitArea.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        audioHandler.Stop();
    }

    public override void _Process(double delta)
    {
        if (isGoingToDie) return;
        if (!MathM.IsInRange(GlobalPosition, player.GlobalPosition, GameManager.GAMEUNITS * distanceToPlayer))
        {
            DeActivate();
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        if (isGoingToDie) return;

        VisionCast.LookAt(player.GlobalPosition);

        if (navAgent.IsNavigationFinished()) return;

        animationHandler.Play("walk");

        Vector2 nextPos = navAgent.GetNextPathPosition();
        Vector2 dir = GlobalPosition.DirectionTo(nextPos);

        if (dir.X > 0) GetNode<Sprite2D>("Sprite").FlipH = true;
        else GetNode<Sprite2D>("Sprite").FlipH = false;

        Velocity = dir * Speed * GameManager.GAMEUNITS * (float)delta;
        audioHandler.PlayWalk();
        MoveAndSlide();
    }

    public void Damage(float modifier, int amount = 1)
    {
        if (isGoingToDie) return;

        lifeSystem.GetDamage(modifier, amount);
        animationHandler.Damage();
        audioHandler.PlayHit();
    }

    void NavUpdate() {
        if(isInVision) navAgent.TargetPosition = player.GlobalPosition;
        NavUpdateTimer.Start();
    }

    void Die()
    {
        animationHandler.Die();
        Timer toDie = NodeMisc.GenTimer(this, (float)animationHandler.GetAnimationTime(), () =>
        {
            if (itemThatMightDrop != null)
            {
                byte chance = (byte)GameManager.rnd.Next(1, 100);
                if(itemDropPercentage >= chance)
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
        isGoingToDie = true;
    }
    
    void WhenHit(Node body)
    {
        if(!fightSystem.canAttack || isGoingToDie) return;
        if (body is Player p)
        {
            p.Damage(damage);
            fightSystem.Attack();
        }
    }
}
