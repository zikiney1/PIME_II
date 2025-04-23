using Godot;
using System;

public class LifeSystem{
    EquipamentSys equipamentSys;
    byte currentLife;
    byte maxLife;
    public Action WhenDies;
    public LifeSystem(EquipamentSys equipamentSystem, byte currentLife, byte maxLife){
        this.equipamentSys = equipamentSystem;
        this.currentLife = currentLife;
        this.maxLife = maxLife;
        WhenDies = () => {};
    }
    public void GetDamage(Equipament[] enemyEquipaments, sbyte amount = 1){
        GetDamage(equipamentSys.GetDamageModifier(enemyEquipaments),amount);
    }
    public void GetDamage(Element element, sbyte amount = 1){
        GetDamage(equipamentSys.GetDamageModifier(element),amount);
    }
    public void GetDamage(float modifier, sbyte amount = 1){
        if(modifier < 0) modifier = 1;
        byte totalDamage = (byte)(amount * Math.Floor(modifier));

        if(currentLife - totalDamage <= 0) {
            if(WhenDies != null) WhenDies();
            return;
        }
        currentLife -= totalDamage;
    }

    public int CurrentLife() => currentLife;
    public int MaxLife() => maxLife;
}