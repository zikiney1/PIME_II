using System;
using Godot;




public class EquipamentSys{
    public ItemResource equipament;
    public float speed = 0;
    public float attackSpeed = 0;
    public float defense = 0;
    public float damage = 0;

    public EquipamentSys(){
    }
    public bool AddEquipament(byte id) => AddEquipament(ItemDB.GetItemData(id));
    public bool AddEquipament(ItemResource equipamentItem){
        if(equipamentItem == null || equipamentItem.equipamentData == null) return false;
        if(equipament != null) return false;

        speed += equipamentItem.equipamentData.SpeedModifier;
        attackSpeed += equipamentItem.equipamentData.AttackSpeedModifier;
        defense += equipamentItem.equipamentData.DefenseModifier;
        damage += equipamentItem.equipamentData.DamageModifier;

        equipament = equipamentItem;
        return true;
    }
    public void RemoveEquipament(){
        speed = 0;
        attackSpeed = 0;
        defense = 0;
        damage = 0;

        equipament = null;
    }

    public string IdData()
    {
        if (equipament == null) return "";
        return equipament.id + "";
    }


    
}