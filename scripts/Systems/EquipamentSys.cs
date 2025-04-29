using System;




public class EquipamentSys{
    public EquipamentData[] equipaments;
    byte lastInserted = 0;
    public EntitieModifier EntitieModifier;

    public EquipamentSys(EntitieModifier EntitieModifier){
        equipaments = new EquipamentData[2];
        this.EntitieModifier = EntitieModifier;
    }

    public bool AddEquipament(EquipamentData equipament){
        if(lastInserted == 2 || equipament == null) return false;

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
        return true;
    }
    public void RemoveEquipament(int index){
        if(index >= lastInserted || index < 0) throw new Exception("Index out of range");
        
        equipaments[index] = null;
    }
    
    
    public void RemoveAllEquipaments(){
        for(int i = 0; i < lastInserted; i++){
            equipaments[i] = null;
        }
        lastInserted = 0;
    }


    
}