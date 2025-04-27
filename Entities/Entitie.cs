using System;
using Godot;

public partial class Entitie : CharacterBody2D{

    protected float Speed = (GameManager.GAMEUNITS)  * 1000;

    protected LifeSystem lifeSystem;
    protected InventorySystem inventory;
    public EntitieModifier entitieModifier;
    protected EquipamentSys equipamentSys;
    protected Vector2 lastDirection;

    protected ItemData HandItem = null;

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
    public void Damage(int amount = 1) => lifeSystem.GetDamage(1f,amount);
    public void Damage(EntitieModifier entitieModifier,int amount = 1) => lifeSystem.GetDamage(entitieModifier,amount);
    public void Damage(ElementsEnum element,sbyte amount = 1) => lifeSystem.GetDamage(element,amount);
    public int MaxLife() => lifeSystem.MaxLife();
    public int CurrentLife() => lifeSystem.CurrentLife();
    public void Heal(int amount = 1) => lifeSystem.Heal(amount);


    protected virtual void Attack(){}
    protected virtual void Defend(){}
    protected virtual void Die(){}
    protected virtual void UsePotion(){}
    protected virtual void StopAttack(){}

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
}