using Godot;
using System;

public partial class EspadachinPlanta : Area2D
{
    GameManager manager;
    Player player;
    [Export] int rayCastDistanceInTiles = 10;
    [Export] byte totalLife = 1;
    [Export] byte damage = 1;
    [Export] float speed = 40;
    [Export] float timeToAct = 1.5f;
    [Export] byte coinsToDrop = 1;
    RayCast2D rayCast;
    VisibleOnScreenNotifier2D visibleNotifier;
    AudioHandler audioHandler;

    Timer timerToAct;
    Timer DieTimer;

    LifeSystem lifeSystem;
    AnimationHandler animationHandler;

    Vector2 playerPos;

    bool followLastPlayerPos = false;
    bool isGoingToDie = false;
    float targetAngle = 0;

    public override void _Ready()
    {
        player = Player.Instance;
        manager = GameManager.Instance;

        lifeSystem = new(totalLife, totalLife);
        lifeSystem.WhenDies += Die;

        animationHandler = new(GetNode<AnimationPlayer>("Animation/AnimationPlayer"), GetNode<AnimationPlayer>("Animation/HitAnimationPlayer"));
        audioHandler = GetNode<AudioHandler>("AudioHandler");

        visibleNotifier = new();
        visibleNotifier.ScreenEntered += Activate;
        visibleNotifier.ScreenExited += DeActivate;

        rayCast = new()
        {
            CollisionMask = this.CollisionMask,
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };
        timerToAct = NodeMisc.GenTimer(this, timeToAct, Act);
        DieTimer = NodeMisc.GenTimer(this,1, Die);
        BodyEntered += WhenHit;

        AddChild(visibleNotifier);
        AddChild(rayCast);
        DeActivate();
    }

    void Activate()
    {
        SetPhysicsProcess(true);
        SetProcess(true);
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        GetNode<Sprite2D>("Sprite").Visible = true;
        Monitoring = true;
        Monitorable = true;
    }
    void DeActivate()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        GetNode<Sprite2D>("Sprite").Visible = false;
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
        Monitoring = false;
        Monitorable = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isGoingToDie) return;
        if (followLastPlayerPos)
        {
            GlobalPosition += (playerPos - GlobalPosition).Normalized() * speed * GameManager.GAMEUNITS * (float)delta;
            if (MathM.IsInRange(GlobalPosition, playerPos, 5f))
            {
                followLastPlayerPos = false;
                if (DieTimer.IsStopped())
                {
                    DieTimer.Start();
                    isGoingToDie = true;
                }
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

    public void Damage(float modifier, int amount = 1)
    {
        lifeSystem.GetDamage(modifier, amount);
        animationHandler.Damage();
        audioHandler.PlayHit();
    }

    void Act()
    {
        playerPos = player.GlobalPosition;
        followLastPlayerPos = true;
        audioHandler.PlayShoot();
    }
    void Die()
    {
        animationHandler.Die();
        Timer toDie = NodeMisc.GenTimer(this, (float)animationHandler.GetAnimationTime(), () =>
        {
            if (!isGoingToDie) manager.SpawnCoins(GlobalPosition, coinsToDrop);
            else manager.SpawnCoins(GlobalPosition, coinsToDrop / 2);

            QueueFree();
        });
        toDie.Start();
        audioHandler.PlayDie();
        
    }
    void WhenHit(Node2D body)
    {
        if(body is Player p) p.Damage(damage);
    }
}
