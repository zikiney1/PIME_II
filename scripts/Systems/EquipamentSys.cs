using System;
using System.Linq;
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

    public EquipamentSys(){
        equipaments = new Equipament[2];
    }

    public void AddEquipament(Equipament equipament){
        if(lastInserted == 2) throw new Exception("Can't add more than 2 equipaments");
        if(equipament == null) throw new Exception("Can't add null equipament");
 
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

    public sbyte GetDamage(Equipament[] enemyEquipaments, int amount = 1){
        float damageModifier = 0;
        sbyte totalModifier;
        foreach (Equipament enemyEquipament in enemyEquipaments){
            if(enemyEquipament == null) continue;
            foreach (Equipament equipament in equipaments){
                if(equipament == null || enemyEquipament.DamageModifier <= 0) continue;
                //da o dano do equipamento
                damageModifier += equipament.DamageModifier;

                //amplifica ou diminui o dano do equipamento baseado na fraqueza do elemnto ou resistencia dele
                if(equipament.Weaknesses().Contains(enemyEquipament.Type())){
                    damageModifier += enemyEquipament.element.DamageModifier();
                }else if(equipament.Resistances().Contains(enemyEquipament.Type())){
                    damageModifier -= equipament.element.DefenseModifier();
                }
            }
        }
        totalModifier = (sbyte)Math.Round(amount *  damageModifier);

        if(totalModifier > 0) totalModifier = 0;
        return totalModifier;
    }

    public sbyte AttackDamage(Equipament[] enemyEquipaments, int amount = 1){
        float damageModifier = 0;
        sbyte totalModifier;
        foreach (Equipament enemyEquipament in enemyEquipaments){
            if(enemyEquipament == null) continue;
            foreach (Equipament equipament in equipaments){
                if(equipament == null || enemyEquipament.DamageModifier <= 0) continue;
                //da o dano do equipamento
                damageModifier += equipament.DamageModifier;

                //amplifica ou diminui o dano do equipamento baseado na fraqueza do elemnto ou resistencia dele
                if(enemyEquipament.Weaknesses().Contains(equipament.Type())){
                    damageModifier += equipament.element.DamageModifier();
                }else if(enemyEquipament.Resistances().Contains(equipament.Type())){
                    damageModifier -= enemyEquipament.element.DefenseModifier();
                }
            }
        }
        totalModifier = (sbyte)Math.Round(amount *  damageModifier);

        if(totalModifier < 0) totalModifier = 0;
        return totalModifier;
    }

    

    public sbyte GetSpeedModifier(){
        if(lastInserted == 0) return 0;
        float EquipamentModifier = 0;
        for(int i = 0; i < lastInserted; i++){
            if(equipaments[i] == null) continue;
            EquipamentModifier += equipaments[i].SpeedModifier;
            
        }
        return (sbyte)(1 + Math.Round(EquipamentModifier));
    }

    
}