using Godot;
using System;

public partial class Boss : CharacterBody2D
{
    [Export] public int damage = 3;
    [Export] Texture2D BulletTexture;

    [ExportGroup("Audio")]
    [Export] public AudioStream ShootSound;
    [Export] public AudioStream PisadaSound;
    [Export] public AudioStream PisadaRocksSound;
    [Export] public AudioStream WalkSound;
    [Export] public AudioStream GiradaSound;
    [Export] public AudioStream DieSound;

    public static Boss Instance;

    enum BossStates
    {
        Lock,
        idle,
        pisada,
        lançaEspinhos,
        Giratoria
    }
    BossStates bossState = BossStates.idle;
    BossStates lastBossState = BossStates.idle;
    Timer StateUpdaterTimer;
    Timer inStateCoolDown;
    Action whenStateCoolDownEnds;
    int timesInTheState = 0;
    AnimationHandler animationHandler;
    AudioStreamPlayer2D audioPlayer;
    AnimationPlayer effectsPlayer;

    Sprite2D effectSprite;
    Vector2 effectSpriteInitialPos;
    Area2D HitArea;
    CollisionShape2D HitAreaCollision;
    Player player;
    GameManager manager;
    LifeSystem lifeSystem = new(50,50);
    Random rnd = new();

    Timer pisadaUpdater;
    RectangleShape2D pisadaShape;

    Timer atirarCoolDownTimer;
    Timer temporizadorDeTiro; //quanto tempo até parar de atirar
    float fireRate = 0.2f;
    float bulletSpeed = 300f;
    bool isShootingNormal = true;
    bool isToStopShooting = false;

    const float rotationSpeed = 900;
    const float rotationTime = 4;
    float curSpeed = 0;
    Tween tween;

    public override void _EnterTree()
    {
        base._EnterTree();
        audioPlayer = GetNode<AudioStreamPlayer2D>("AudioPlayer");
        Instance = this;
        HitArea = GetNode<Area2D>("HitArea");
        HitAreaCollision = HitArea.GetNode<CollisionShape2D>("CollisionShape2D");

        HitArea.BodyEntered += Hit;
        GetNode<Area2D>("CollisionArea").BodyEntered += Hit;
        GetNode<CollisionShape2D>("CollisionArea/CollisionShape2D").Disabled = true;

        effectSprite = HitArea.GetNode<Sprite2D>("effects");
        effectSpriteInitialPos = effectSprite.Position;
        effectsPlayer = GetNode<AnimationPlayer>("Animations/EffectsAnimationPlayer");

        StateUpdaterTimer = NodeMisc.GenTimer(this, 4, StateUpdate);
        inStateCoolDown = NodeMisc.GenTimer(this, 2f, () => whenStateCoolDownEnds?.Invoke());

        pisadaUpdater = NodeMisc.GenTimer(this, 0.1f, OnPisadaUpdate);

        atirarCoolDownTimer = NodeMisc.GenTimer(this, fireRate, Atirar);
        temporizadorDeTiro = NodeMisc.GenTimer(this, 5f, ShootEnd);

        animationHandler = new(
            GetNode<AnimationPlayer>("Animations/SpriteAnimationPlayer"),
            GetNode<AnimationPlayer>("Animations/HitAnimationPlayer")
        );
        lifeSystem.WhenDies += Die;
    }

    public override void _Ready()
    {
        player = Player.Instance;
        manager = GameManager.Instance;
        
    }

    public void Activate()
    {
        StateUpdaterTimer.Start();
        GetNode<CollisionShape2D>("CollisionArea/CollisionShape2D").Disabled = false;

    }


    public override void _Process(double delta)
    {
        if (bossState == BossStates.Giratoria && curSpeed > 0)
        {
            HitArea.RotationDegrees += (float)(curSpeed * delta);
            GlobalPosition += (player.Position - GlobalPosition).Normalized() * GameManager.GAMEUNITS /4 * (float)delta;
        }
        if (bossState == BossStates.pisada && !inStateCoolDown.IsStopped())
        {
            Vector2 direction = GetDir();
            GlobalPosition += direction * GameManager.GAMEUNITS * 2 * (float)delta;
            animationHandler.Direction(MathM.RoundedVector(direction));
        }
    }

    void PlaySFX(AudioStream stream)
    {
        try{
            audioPlayer.Stop();
            audioPlayer.Stream = stream;
            audioPlayer.Play();
        }catch{}
    }

    Vector2 GetDir() => (player.Position - GlobalPosition).Normalized();


    void StateUpdate()
    {
        if(bossState == BossStates.Lock) return;
        int newRandomState = rnd.Next(1, 4);
        timesInTheState = 0;
        if (newRandomState == 1)
        {
            bossState = BossStates.pisada;
            if (bossState == lastBossState) StateUpdate();
            else PisadaStart();

        }
        else if (newRandomState == 2)
        {
            bossState = BossStates.lançaEspinhos;
            if (bossState == lastBossState) StateUpdate();
            else ShootStart();
        }
        else
        {
            bossState = BossStates.Giratoria;
            if (bossState == lastBossState) StateUpdate();
            else StartRotation();
        }
    }

    void restart()
    {
        lastBossState = bossState;
        bossState = BossStates.idle;
        StateUpdaterTimer.Start();
    }


    // ================================ pisada =====================================
    void PisadaStart()
    {
        effectSprite.Position = effectSpriteInitialPos;
        effectSprite.RotationDegrees = -90;
        if (GetDir().Y < 0)
            animationHandler.Play("pisada_down");
        else
            animationHandler.Play("pisada_up");

        Timer tm = new();


        tm = NodeMisc.GenTimer(this, (float)animationHandler.GetAnimationTime()/2, () =>
        {
            HitArea.LookAt(player.GlobalPosition);
            HitArea.Rotate(1.57f);

            pisadaShape = new RectangleShape2D()
            {
                Size = new Vector2(32, 0)
            };

            HitAreaCollision.Shape = pisadaShape;
            HitAreaCollision.Position = new Vector2(0, 0);
            HitAreaCollision.Disabled = false;
            pisadaUpdater.Start();

            tm.QueueFree();
            PlaySFX(PisadaSound);
            effectsPlayer.Play("pedras");
        });
        tm.Start();

    }

    void PisadaEnd()
    {
        timesInTheState++;
        HitAreaCollision.Disabled = true;

        if (timesInTheState >= 3)
        {
            restart();
        }
        else
        {
            whenStateCoolDownEnds = () =>
            {
                PisadaStart();
            };
            inStateCoolDown.Start();
            PlaySFX(WalkSound);
        }
    }

    void OnPisadaUpdate()
    {
        if (pisadaShape.Size.Y >= GameManager.GAMEUNITS * 5)
        {
            PisadaEnd();
        }
        else
        {
            pisadaShape.Size = new Vector2(pisadaShape.Size.X, pisadaShape.Size.Y + GameManager.GAMEUNITS);
            HitAreaCollision.Shape = pisadaShape;
            HitAreaCollision.Position = new Vector2(0, -pisadaShape.Size.Y / 2);

            PlaySFX(PisadaRocksSound);

            pisadaUpdater.Start();
        }
    }
    // ================================ pisada =====================================
    // ============================ Lanca Espinhos =================================

    void ShootStart()
    {
        isToStopShooting = false;
        atirarCoolDownTimer.Start();
        temporizadorDeTiro.Start();
        animationHandler.Play("ball_start");
    }

    void ShootEnd()
    {
        isToStopShooting = true;
        atirarCoolDownTimer.Stop();
        animationHandler.Play("ball_end");
        restart();
    }

    void Atirar()
    {
        if(isToStopShooting) return;

        PlaySFX(ShootSound);

        if (isShootingNormal)
        {
            ShootNormal();
        }
        else
        {
            ShootDiagonal();
        }
        isShootingNormal = !isShootingNormal;
        atirarCoolDownTimer.Start();
    }

    void ShootNormal()
    {
        shoot(MathM.DegreeToVec2(0));
        shoot(MathM.DegreeToVec2(90));
        shoot(MathM.DegreeToVec2(180));
        shoot(MathM.DegreeToVec2(270));
    }

    void ShootDiagonal()
    {
        shoot(MathM.DegreeToVec2(45));
        shoot(MathM.DegreeToVec2(135));
        shoot(MathM.DegreeToVec2(225));
        shoot(MathM.DegreeToVec2(315));
    }

    void shoot(Vector2 direction)
    {
        var e = manager.GetBullet(GameManager.EnemyBulletMask,GlobalPosition,direction);
        e.SetTexture(BulletTexture);
        e.speed = bulletSpeed;
        e.WhenBodyEnter = Hit;
    }

    // ============================ Lanca Espinhos =================================
    // =============================== Giratoria ===================================
    private async void StartRotation()
    {
        curSpeed = 0f;
        HitArea.RotationDegrees = 0;
        effectSprite.Position = Vector2.Zero;
        effectSprite.Scale = new Vector2(2, 2);
        effectSprite.RotationDegrees = 0;
        effectsPlayer.Play("laminaStart");
        animationHandler.Play("ball_start");

        HitAreaCollision.Disabled = false;
        HitAreaCollision.Position = Vector2.Zero;
        HitAreaCollision.Shape = new RectangleShape2D()
        {
            Size = new Vector2(320, 32)
        };
        PlaySFX(GiradaSound);

        // Aceleração suave
        tween?.Kill();
        tween = GetTree().CreateTween();
        tween
            .TweenProperty(this, "curSpeed", rotationSpeed, rotationTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        await ToSignal(tween, "finished");

        if (bossState == BossStates.Lock)
        {
            StopRotation();
            return;
        }

        // Mantém velocidade máxima por um tempo
        await ToSignal(GetTree().CreateTimer(rotationTime), "timeout");

        if (bossState == BossStates.Lock)
        {
            StopRotation();
            return;
        }

        // Desaceleração suave
        tween = GetTree().CreateTween();
        tween
            .TweenProperty(this, "curSpeed", 0f, rotationTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        await ToSignal(tween, "finished");

        
        StopRotation();
        restart();
    }

    void StopRotation()
    {
        tween?.Kill();
        curSpeed = 0f;
        HitAreaCollision.Disabled = true;
        HitArea.RotationDegrees = 0;
        effectSprite.Scale = Vector2.One;
        audioPlayer.Stop();
        effectsPlayer.Play("laminaEnd");
        animationHandler.Play("ball_end");
    }
    // =============================== Giratoria ===================================

    void Hit(Node2D body)
    {
        if (body is Player p)
        {
            p.Damage(damage);
        }
    }
    void Die()
    {
        bossState = BossStates.Lock;
        StateUpdaterTimer.Stop();
        pisadaUpdater.Stop();
        atirarCoolDownTimer.Stop();
        temporizadorDeTiro.Stop();

        Timer timeTodie = NodeMisc.GenTimer(this, 6f, () =>
        {
            GetTree().ChangeSceneToFile("res://GUI/TelasPrincipais/TelaCreditos.tscn");
        });
        timeTodie.Start();
        animationHandler.Play("ball_start");
        animationHandler.SetVel(0.3f);
        Player.Instance.WinState();

        GD.Print("BOSS DIED");
        
        PlaySFX(DieSound);
    }

    public void Damage(float modifier, int amount = 1)
    {
        GD.Print("Boss life: " + lifeSystem.CurrentLife());
        lifeSystem.GetDamage(modifier, amount);
        animationHandler.Damage();
    }
}
