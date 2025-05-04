using System;
using Godot;

public partial class Entitie : CharacterBody2D{

    protected float Speed = (GameManager.GAMEUNITS)  * 1000;

    protected LifeSystem lifeSystem;
    public EntitieModifier entitieModifier;

    
    protected Vector2 lastDirection;

    protected Slot HandItem = null;

    protected Timer DamageTimer;
    float InvencibleTimer = 1f;
    float AttackSpeed = 0.3f;
    protected Timer AttackTimer;

    protected bool canReciveDamage = true;


    public enum EntitieState{
        Idle,
        Walking,
        Attacking,
        Dead,
    }
    protected EntitieState state = EntitieState.Idle;
    protected EntitieState previousState;

    public void SetPotionDamageModifier(Half modifier, ElementsEnum element) => entitieModifier.SetPotionDamageModifier(modifier,element);
    public void SetPotionResistenceModifier(Half modifier, ElementsEnum element) => entitieModifier.SetPotionResistenceModifier(modifier,element);
    public void SetPotionSpeedModifier(Half modifier) => entitieModifier.SetPotionSpeedModifier(modifier);
    public int MaxLife() => lifeSystem.MaxLife();
    public int CurrentLife() => lifeSystem.CurrentLife();
    public void Heal(int amount = 1) => lifeSystem.Heal(amount);

    public void Damage(int amount = 1) {
        if(!canReciveDamage) return;
        lifeSystem.GetDamage(1f,amount);
     
        WhenTakeDamage();
        DamageTimer.Start();
    }
    public void Damage(EntitieModifier entitieModifier,int amount = 1) {
        if(!canReciveDamage) return;
        lifeSystem.GetDamage(entitieModifier,amount);
     
        WhenTakeDamage();
        DamageTimer.Start();
    }
    public void Damage(ElementsEnum element,int amount = 1){
        if(!canReciveDamage) return;
        lifeSystem.GetDamage(element,amount);
     
        WhenTakeDamage();
        DamageTimer.Start();
    }



    protected virtual void Defend(){}
    protected virtual void Die(){}
    
    protected virtual void Ready_(){}
    
    
    
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

    protected virtual void Attack(){
        if(lastDirection == Vector2.Zero || state == EntitieState.Attacking) return;
        previousState = state;
        this.state = EntitieState.Attacking;
        AttackTimer.Start();
    }


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