using Godot;
using System;

public partial class Atirador : StaticBody2D
{
    [Export] GameManager gameManager;
    [Export] Player player;
    [Export] Texture2D BulletTexture;
    [Export] float attackSpeed = 0.5f;
    [Export] int BulletDamage = 1;
    [Export] int bulletQuantity = 1;
    [Export] float bulletSpeed = 300f;
    [Export] int rayCastDistanceInTiles = 8;
    [Export] byte life = 10;

    RayCast2D rayCast;
    Timer FireTimer;
    VisibleOnScreenNotifier2D visibleNotifier;


    int currentBullet = 0;
    LifeSystem lifeSystem;
    FightSystem fightSystem;


    public override void _Ready()
    {
        base._Ready();
        lifeSystem = new(life, life);
        fightSystem = new(this, attackSpeed);

        rayCast = new()
        {
            TargetPosition = new Vector2(rayCastDistanceInTiles * GameManager.GAMEUNITS, 0)
        };

        FireTimer = NodeMisc.GenTimer(this, 0.1f, Fire);
        visibleNotifier = new();

        AddChild(visibleNotifier);
        AddChild(rayCast);

    }

    public override void _Process(double delta)
    {
        rayCast.LookAt(player.GlobalPosition);
        if (rayCast.IsColliding())
        {
            LookAt(player.GlobalPosition);
            Attack();
        }
    }

    public void Attack()
    {
        if (!fightSystem.canAttack) return;

        FireTimer.Start();

        fightSystem.Attack(0);
    }

    void Fire()
    {
        gameManager.EnemyShoot(GlobalPosition, (player.GlobalPosition - GlobalPosition).Normalized() * bulletSpeed);

        currentBullet++;
        if (currentBullet >= bulletQuantity)
        {
            currentBullet = 0;
            FireTimer.Stop();
        }
        else
        {
            FireTimer.Start();
        }
    }

//player.Damage(BulletDamage);


}
