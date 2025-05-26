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

    LifeSystem lifeSystem;
    FightSystem fightSystem;
    AnimationHandler animationHandler;

    Player player;
    GameManager manager;

    VisibleOnScreenNotifier2D visibleNotifier;
    NavigationAgent2D navAgent;

    RayCast2D VisionCast;
    Timer NavUpdateTimer;
    Area2D HitArea;

    bool isInVision => VisionCast.IsColliding() && VisionCast.GetCollider() == player;

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
    }

    void Activate()
    {
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
    }

    public override void _Process(double delta)
    {
        if (!MathM.IsInRange(GlobalPosition, player.GlobalPosition, GameManager.GAMEUNITS * distanceToPlayer))
        {
            DeActivate();
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        VisionCast.LookAt(player.GlobalPosition);

        if (navAgent.IsNavigationFinished()) return;

        Vector2 nextPos = navAgent.GetNextPathPosition();
        Velocity = GlobalPosition.DirectionTo(nextPos) * Speed * GameManager.GAMEUNITS * (float)delta;
        MoveAndSlide();
    }

    public void Damage(float modifier, int amount = 1)
    {
        lifeSystem.GetDamage(modifier, amount);
        animationHandler.Damage();
    }

    void NavUpdate() {
        if(isInVision) navAgent.TargetPosition = player.GlobalPosition;
        NavUpdateTimer.Start();
    }

    void Die()
    {
        manager.SpawnCoins(GlobalPosition, coinsToDrop);
        QueueFree();
    }
    
    void WhenHit(Node body)
    {
        if(!fightSystem.canAttack) return;
        if (body is Player p)
        {
            p.Damage(damage);
            fightSystem.Attack();
        }
    }
}
