using System;

public class Equipament{
    public Equipament(byte id, Element element, float damageModifier, float defenseModifier, float speedModifier){
        this.id = id;
        this.element = element;
        this.DamageModifier = damageModifier;
        this.DefenseModifier = defenseModifier;
        this.SpeedModifier = speedModifier;
    }
    public byte id {get;}
    public Element element { get;}
    public float DamageModifier { get;}
    public float DefenseModifier { get;}
    public float SpeedModifier { get;}

    public ElementsEnum Type() => element.Type();
    public ElementsEnum[] Weaknesses() => element.Weaknesses();
    public ElementsEnum[] Resistances() => element.Resistances();
}


public class EquipamentSys{
    public Equipament[] equipaments;
    byte lastInserted = 0;
    public EntitieModifier EntitieModifier;

    public EquipamentSys(EntitieModifier EntitieModifier){
        equipaments = new Equipament[2];
        this.EntitieModifier = EntitieModifier;
    }

    public void AddEquipament(Equipament equipament){
        if(lastInserted == 2) throw new Exception("Can't add more than 2 equipaments");
        if(equipament == null) throw new Exception("Can't add null equipament");

        foreach (ElementsEnum element in equipament.Resistances()){
            if(element == ElementsEnum.Fire){
                EntitieModifier.fireResistenceModifier += (Half)equipament.DefenseModifier;
                EntitieModifier.fireDamageModifier += (Half)equipament.DamageModifier;
            }
            else if(element == ElementsEnum.Water){
                EntitieModifier.waterResistenceModifier += (Half)equipament.DefenseModifier;
                EntitieModifier.waterDamageModifier += (Half)equipament.DamageModifier;
            }
            else if(element == ElementsEnum.Rock){
                EntitieModifier.rockResistenceModifier += (Half)equipament.DefenseModifier;
                EntitieModifier.rockDamageModifier += (Half)equipament.DamageModifier;
            }
        }
        foreach (ElementsEnum element in equipament.Weaknesses()){
            if(element == ElementsEnum.Fire)
                EntitieModifier.fireResistenceModifier -= (Half)equipament.DefenseModifier;
            else if(element == ElementsEnum.Water)
                EntitieModifier.waterResistenceModifier -= (Half)equipament.DefenseModifier;
            else if(element == ElementsEnum.Rock)
                EntitieModifier.rockResistenceModifier -= (Half)equipament.DefenseModifier;
            
        }


        equipaments[lastInserted++] = equipament;
    }
    public void RemoveEquipament(int index){
        if(index >= lastInserted || index < 0) throw new Exception("Index out of range");
        
        equipaments[index] = null;
    }
    
    public void RemoveEquipament(byte id){
        for(int i = 0; i < lastInserted; i++){
            if(equipaments[i].id == id){
                equipaments[i] = null;
                return;
            }
        }
        throw new Exception("Equipament not found");
    }
    
    public void RemoveAllEquipaments(){
        for(int i = 0; i < lastInserted; i++){
            equipaments[i] = null;
        }
        lastInserted = 0;
    }


    
}