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

    Timer timerToAct;
    Timer DieTimer;

    LifeSystem lifeSystem;

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
        visibleNotifier = NodeMisc.GenVisibleNotifier(this);

        rayCast = new()
        {
            CollisionMask = this.CollisionMask,
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };
        AddChild(rayCast);
        timerToAct = NodeMisc.GenTimer(this, timeToAct, Act);
        DieTimer = NodeMisc.GenTimer(this, timeToAct / 2, Die);
        BodyEntered += WhenHit;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(isGoingToDie) return;
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

    public void Damage(float modifier, int amount=1)
    {
        lifeSystem.GetDamage(modifier, amount);
    }

    void Act()
    {
        playerPos = player.GlobalPosition;
        followLastPlayerPos = true;
    }
    void Die()
    {
        if(!isGoingToDie) manager.SpawnCoins(GlobalPosition, coinsToDrop);
        else manager.SpawnCoins(GlobalPosition, coinsToDrop/2);
        QueueFree();
    }
    void WhenHit(Node2D body)
    {
        if(body is Player p) p.Damage(damage);
    }
}
