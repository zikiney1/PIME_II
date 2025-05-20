using Godot;
using System;

public partial class Atirador : StaticBody2D
{
    GameManager manager;
    Player player;

    [Export] Texture2D BulletTexture;
    [Export] float attackSpeed = 0.5f;
    [Export] int BulletDamage = 1;
    [Export] int bulletQuantity = 1;
    [Export] float fireRate = 0.1f;
    [Export] float bulletSpeed = 300f;
    [Export] float rotationSpeed = 1.0f;
    [Export] int rayCastDistanceInTiles = 8;
    [Export] byte life = 2;
    [Export] byte coinsToDrop = 1;

    RayCast2D rayCast;
    RayCast2D mira;
    Timer FireTimer;
    VisibleOnScreenNotifier2D visibleNotifier;
    Timer lookTimer;


    int currentBullet = 0;
    LifeSystem lifeSystem;
    FightSystem fightSystem;
    AnimationHandler animationHandler;

    float originalRotation = 0;

    bool lookToOrigin = false;
    bool active = true;


    public override void _Ready()
    {
        base._Ready();
        player = Player.Instance;
        manager = GameManager.Instance;

        lifeSystem = new(life, life);
        fightSystem = new(this, attackSpeed);
        lifeSystem.WhenDies += Die;
        animationHandler = new(GetNode<AnimationPlayer>("Animation/AnimationPlayer"), GetNode<AnimationPlayer>("Animation/HitAnimationPlayer"));

        originalRotation = Rotation;

        rayCast = new()
        {
            CollisionMask = this.CollisionMask,
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };
        mira = new()
        {
            CollisionMask = this.CollisionMask,
            TargetPosition = rayCast.TargetPosition
        };

        visibleNotifier = new();
        visibleNotifier.ScreenEntered += Activate;
        visibleNotifier.ScreenExited += DeActivate;

        FireTimer = NodeMisc.GenTimer(this, fireRate, Fire);
        lookTimer = NodeMisc.GenTimer(this, 1f, () => { lookToOrigin = true; });

        AddChild(visibleNotifier);
        AddChild(rayCast);
        AddChild(mira);
        DeActivate();
    }


    void Activate()
    {
        SetPhysicsProcess(true);
        SetProcess(true);
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
        GetNode<Sprite2D>("Sprite").Visible = true;
        
    }
    void DeActivate()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        GetNode<Sprite2D>("Sprite").Visible = false;
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
    }


    public override void _PhysicsProcess(double delta)
    {
        if (!active) return;
        rayCast.LookAt(player.GlobalPosition);

        if (rayCast.IsColliding() && rayCast.GetCollider() == player)
        {
            // Rotaciona até o jogador
            Vector2 toPlayer = player.GlobalPosition - GlobalPosition;
            float targetAngle = toPlayer.Angle();
            Rotation = Mathf.LerpAngle(Rotation, targetAngle, (float)delta * rotationSpeed);
            lookTimer.Stop();
            lookToOrigin = false;

            if (mira.IsColliding() && mira.GetCollider() == player)
                Attack();
        }
        else
        {
            if (!lookToOrigin)
            {
                if (lookTimer.IsStopped())
                    lookTimer.Start();
            }
            else
            {

                Rotation = Mathf.LerpAngle(Rotation, originalRotation, (float)delta * rotationSpeed);
                if (Mathf.IsEqualApprox(Rotation, originalRotation, 0.01f))
                {
                    Rotation = originalRotation;
                    lookToOrigin = false;
                }
            }
        }

    }
    public void Damage(float modifier, int amount = 1)
    {
        lifeSystem.GetDamage(modifier, amount);
        FireTimer.Stop();
        animationHandler.Damage();
    }

    
    public void Die()
    {
        manager.SpawnCoins(GlobalPosition, coinsToDrop);
        QueueFree();
    }


    public void Attack()
    {
        if (!fightSystem.canAttack) return;
        FireTimer.Start();
        fightSystem.Attack();
    }

    void Fire()
    {
        Vector2 direction = new Vector2(Mathf.Cos(Rotation), Mathf.Sin(Rotation));

        var e = manager.GetBullet(GameManager.EnemyBulletMask,GlobalPosition,direction);
        e.SetTexture(BulletTexture);
        e.speed = bulletSpeed;
        e.WhenBodyEnter = WhenEnterBody;
        currentBullet++;

        if (currentBullet >= bulletQuantity){
            currentBullet = 0;
            FireTimer.Stop();
        }
        else{
            FireTimer.Start();
        }
    }

    public void WhenEnterBody(Node2D body)
    {
        if (body is Player p){
            p.Damage(BulletDamage);
        }
    }


}
