using Godot;
using System;

public class LifeSystem{
    EquipamentSys equipamentSys;
    byte totalLife;
    public Action WhenDies;
    public LifeSystem(EquipamentSys equipamentSystem, byte totalLife){
        this.equipamentSys = equipamentSystem;
        this.totalLife = totalLife;
        WhenDies += () => {};
    }

    public void GetDamage(Equipament[] enemyEquipaments, sbyte amount = 1){
        float equipModifier = equipamentSys.GetDamageModifier(enemyEquipaments);
        byte totalDamage = (byte)(amount * Math.Floor(equipModifier));

        if(totalLife - totalDamage <= 0) {
            WhenDies();
            return;
        }
        totalLife -= totalDamage;
    }
}