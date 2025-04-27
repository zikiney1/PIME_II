using Godot;
using System;

public class LifeSystem{
    byte currentLife;
    byte maxLife;
    public Action WhenDies;
    EntitieModifier entitieResistence;
    public LifeSystem(EntitieModifier entitieResistence, byte currentLife, byte maxLife){
        this.currentLife = currentLife;
        this.maxLife = maxLife;
        WhenDies = () => {};
        this.entitieResistence = entitieResistence;
    }
    public void Heal(int amount = 1) => currentLife += (byte)Math.Clamp(amount,1,maxLife-currentLife);

    public void GetDamage(EntitieModifier modifier, int amount = 1){
        float totalModifier = 1;

        totalModifier += modifier.GetFireDamageModifier() - entitieResistence.GetFireResistenceModifier();
        totalModifier += modifier.GetWaterDamageModifier() - entitieResistence.GetWaterResistenceModifier();
        totalModifier += modifier.GetRockDamageModifier() - entitieResistence.GetRockResistenceModifier();

        GetDamage(totalModifier,amount);
    }

    public void GetDamage(ElementsEnum element, int amount = 1){
        if(element == ElementsEnum.Fire) GetDamage((float)entitieResistence.GetFireResistenceModifier(),amount);
        else if(element == ElementsEnum.Water) GetDamage((float)entitieResistence.GetWaterResistenceModifier(),amount);
        else if(element == ElementsEnum.Rock) GetDamage((float)entitieResistence.GetRockResistenceModifier(),amount);
    }

    public void GetDamage(float modifier, int amount = 1){
        byte totalDamage = (byte)(amount * (1 + Math.Floor(modifier)));

        if(currentLife - totalDamage <= 0) {
            if(WhenDies != null) WhenDies();
            return;
        }
        currentLife -= totalDamage;
    }

    public int CurrentLife() => currentLife;
    public int MaxLife() => maxLife;
}