using Godot;
using System;

public class LifeSystem{
    byte currentLife;
    byte maxLife;
    public Action WhenDies;
    public LifeSystem(byte currentLife, byte maxLife){
        this.currentLife = currentLife;
        this.maxLife = maxLife;
        WhenDies = () => {};
    }
    public void Heal(int amount = 1) => currentLife += (byte)Math.Clamp(amount,1,maxLife-currentLife);

    /// <summary>
    /// Applies a damage to the LifeSystem, with an optional modifier to reduce the damage.
    /// If the modifier makes the damage 0 or less, a minimum of 1 damage is applied.
    /// When the LifeSystem reaches 0 life, the WhenDies action is called.
    /// </summary>
    /// <param name="modifier">The modifier to reduce the damage. 0 means no reduction, 1 means total reduction.</param>
    /// <param name="amount">The base amount of damage to apply. Defaults to 1.</param>
    public void GetDamage(float modifier, int amount){
        byte totalDamage = (byte)Math.Floor(amount * (1 - modifier));
        if(totalDamage <= 0){
            totalDamage = 1;
        }

        if(currentLife - totalDamage <= 0) {
            if(WhenDies != null) WhenDies();
            return;
        }
        currentLife -= totalDamage;
    }
    public bool WillBeDead(int amount = 1) => currentLife - amount <= 0;

    public int CurrentLife() => currentLife;
    public int MaxLife() => maxLife;
}