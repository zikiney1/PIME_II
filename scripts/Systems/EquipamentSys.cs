using System;
using Godot;




public class EquipamentSys{
    public ItemResource[] equipaments;
    byte lastInserted = 0;
    public float speed = 0;
    public float attackSpeed = 0;
    public float defense = 0;
    public float damage = 0;

    public EquipamentSys(){
        equipaments = new ItemResource[2];
    }
    public bool AddEquipament(byte id) => AddEquipament(ItemDB.GetItemData(id));
    public bool AddEquipament(ItemResource equipamentItem){
        if(lastInserted == 2 || equipamentItem == null || equipamentItem.equipamentData == null) return false;

        speed += equipamentItem.equipamentData.SpeedModifier;
        attackSpeed += equipamentItem.equipamentData.AttackSpeedModifier;
        defense += equipamentItem.equipamentData.DefenseModifier;
        damage += equipamentItem.equipamentData.DamageModifier;


        equipaments[lastInserted++] = equipamentItem;
        return true;
    }
    public void RemoveEquipament(int index){
        if(index >= lastInserted || index < 0) {
            GD.PushError("index out of range");
            return;
        }

        speed -= equipaments[index].equipamentData.SpeedModifier;
        attackSpeed -= equipaments[index].equipamentData.AttackSpeedModifier;
        defense -= equipaments[index].equipamentData.DefenseModifier;
        damage -= equipaments[index].equipamentData.DamageModifier;
        
        equipaments[index] = null;
    }
    
    
    public void RemoveAllEquipaments(){
        for(int i = 0; i < lastInserted; i++){
            equipaments[i] = null;
        }
        lastInserted = 0;
        speed = 0;
        attackSpeed = 0;
        defense = 0;
        damage = 0;
    }


    
}