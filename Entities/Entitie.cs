using System;
using Godot;

/// <summary>
/// Classe base para entidades do jogo que usam física de personagem 2D.
/// </summary>
public partial class Entitie : CharacterBody2D{

    protected float Speed = (GameManager.GAMEUNITS)  * 1000;

    public LifeSystem lifeSystem;

    protected Vector2 lastDirection;
    protected Timer DamageTimer;
    protected float InvencibleTimer = 1f;
    protected float AttackSpeed = 0.3f;
    protected Timer AttackTimer;
    protected bool canReciveDamage = true;


    public enum EntitieState{
        Idle,
        Walking,
        Attacking,
        Dead,
        Lock
    }
    protected EntitieState state = EntitieState.Idle;
    protected EntitieState previousState;

    public int MaxLife() => lifeSystem.MaxLife();
    public int CurrentLife() => lifeSystem.CurrentLife();
    public void Heal(int amount = 1) => lifeSystem.Heal(amount);

    /// <summary>
    /// Aplica dano à entidade se não estiver invencível.
    /// </summary>
    public void Damage(float modifier,int amount = 1) {
        if(!canReciveDamage) return;
        lifeSystem.GetDamage(modifier,amount);
     
        WhenTakeDamage();
        DamageTimer.Start();
    }



    protected virtual void Defend(){}
    protected virtual void Die(){}
    
    protected virtual void Ready_(){}
    
    
    
    /// <summary>
    /// Para o ataque e retorna ao estado anterior.
    /// </summary>
    protected virtual void StopAttack(){
        this.state = EntitieState.Idle;
        state = previousState;
    }

    public override void _Ready(){
        base._Ready();
        lastDirection = new();
        previousState = state;

        DamageTimer = NodeMisc.GenTimer(this, InvencibleTimer, WhenInvencibleTimerEnds);
        AttackTimer = NodeMisc.GenTimer(this, AttackSpeed, StopAttack);
        Ready_();
    }

    /// <summary>
    /// Inicia um ataque, se possível.
    /// </summary>
    protected virtual void Attack(){
        if(lastDirection == Vector2.Zero || state == EntitieState.Attacking) return;
        previousState = state;
        this.state = EntitieState.Attacking;
        AttackTimer.Start();
    }

    /// <summary>
    /// Move a entidade em uma direção com determinada velocidade.
    /// </summary>
    protected virtual void Walk(Vector2 direction,double delta, float speed = 0){
        if (speed == 0) speed = Speed;
        if(direction != Vector2.Zero){
            state = EntitieState.Walking;
            Velocity = direction * speed * (float)delta;
            lastDirection = direction;
        }else{
            state = EntitieState.Idle;
            Velocity = Vector2.Zero;
        }

		MoveAndSlide();
    }

    public virtual void WhenTakeDamage(){}
    public virtual void TakeDamageUpdate(){}
    public virtual void WhenTakeDamageTimerEnds(){}

    public virtual void whenHeal(){}
    public virtual void HealUpdate(){}

    public virtual void DamageUpdate(){}
    public virtual void EffectUpdate(){}

    protected virtual void WhenInvencibleTimerEnds(){}

}