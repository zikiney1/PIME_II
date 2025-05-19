using Godot;
using System;

public partial class CachorroPlanta : CharacterBody2D
{
    [Export] public float attackSpeed;
    [Export] public byte totalLife;
    [Export] public byte coinsToDrop;
    [Export] public float distanceToPlayer;
    [Export] public float Speed = 230;
    LifeSystem lifeSystem;
    Player player;
    VisibleOnScreenNotifier2D visibleNotifier;
    NavigationAgent2D navAgent;

    RayCast2D VisionCast;


    public override void _Ready()
    {
        base._Ready();
        player = Player.Instance;
        lifeSystem = new(totalLife, totalLife);
        lifeSystem.WhenDies += Die;
        // visibleNotifier = NodeMisc.GenVisibleNotifier(this);

        VisionCast = new()
        {
            CollisionMask = this.CollisionMask,
            TargetPosition = new Vector2(distanceToPlayer * GameManager.GAMEUNITS, 0)
        };
        AddChild(VisionCast);

        navAgent = GetNode<NavigationAgent2D>("NavigationAgent2D");
        navAgent.TargetPosition = player.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (navAgent.IsNavigationFinished()) return;
        

        navAgent.TargetPosition = player.GlobalPosition;
        Vector2 nextPos = navAgent.GetNextPathPosition();
        Vector2 vel = GlobalPosition.DirectionTo(nextPos) * Speed * GameManager.GAMEUNITS * (float)delta;

        if (navAgent.AvoidanceEnabled)
            navAgent.SetVelocityForced(vel);
        else
            NavAgentVelCompute(vel);


        MoveAndSlide();
    }

    void NavAgentVelCompute(Vector2 safe_vel)
    {
        Velocity = safe_vel;        
    }


    void Die()
    {
        
    }
}
