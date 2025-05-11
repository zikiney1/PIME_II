using Godot;
using System;

public class LifeSystem{
    byte currentLife;
    byte maxLife;
    public Action WhenDies;
    // EntitieModifier entitieResistence;
    public LifeSystem(byte currentLife, byte maxLife){
        this.currentLife = currentLife;
        this.maxLife = maxLife;
        WhenDies = () => {};
    }
    public void Heal(int amount = 1) => currentLife += (byte)Math.Clamp(amount,1,maxLife-currentLife);

    public void GetDamage(EntitieModifier entitieModifier, EntitieModifier otherModifier, int amount = 1){
        float totalModifier = 1;

        totalModifier += otherModifier.GetFireDamageModifier() - entitieModifier.GetFireResistenceModifier();
        totalModifier += otherModifier.GetWaterDamageModifier() - entitieModifier.GetWaterResistenceModifier();
        totalModifier += otherModifier.GetRockDamageModifier() - entitieModifier.GetRockResistenceModifier();

        GetDamage(totalModifier,amount);
    }

    public void GetDamage(EntitieModifier entitieResistence,ElementsEnum element, int amount = 1){
        if(element == ElementsEnum.Fire) GetDamage((float)entitieResistence.GetFireResistenceModifier(),amount);
        else if(element == ElementsEnum.Water) GetDamage((float)entitieResistence.GetWaterResistenceModifier(),amount);
        else if(element == ElementsEnum.Rock) GetDamage((float)entitieResistence.GetRockResistenceModifier(),amount);
    }

    public void GetDamage(float modifier, int amount = 1){
        byte totalDamage = (byte)Math.Floor(amount * (1 - modifier));

        if(currentLife - totalDamage <= 0) {
            if(WhenDies != null) WhenDies();
            return;
        }
        currentLife -= totalDamage;
    }

    public int CurrentLife() => currentLife;
    public int MaxLife() => maxLife;
}