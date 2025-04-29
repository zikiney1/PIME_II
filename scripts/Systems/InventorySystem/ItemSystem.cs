using Godot;
using System;
using System.Collections.Generic;
public static class ItemDB{
    public static Dictionary<byte, ItemData> itemDB = new();

    public static void SetupItemDB(){
        const string path = "res://Resources/Items/";
        DirAccess dir = DirAccess.Open(path);
        if(dir == null) return;
        dir.ListDirBegin();
        string fileName;
        while((fileName = dir.GetNext()) != ""){
            ItemResource item = GD.Load<ItemResource>(path + "/" + fileName);

            if(itemDB.ContainsKey(item.id))continue;

            int level = item.PotionEffect == null ? 1 : item.PotionEffect.useLevel? item.level : 1;

            ItemData data = new(
                item.name, 
                item.description,
                item.iconFile, 
                (Half)item.price, 
                item.stackMaxSize,
                item.type, 
                GetPotionEffect(item.PotionEffect, level),
                Equipament.GetEquipament(item.equipamentData, item.id)
            );
            
            itemDB.Add(item.id, data);
        }   
    }
    
    public static PotionEffect GetPotionEffect(PotionEffectResource resource,int level = 1){
        if (resource == null) return null;

        PotionBuilder pb = new (resource.potionType, resource.duration);
        if(resource.healAmount > 0){
            if(resource.HealBehavior == PotionBuilder.PotionType.Instant){
                pb.HealInstant(AmountWithLevel(resource.healAmount,level));
            }else{
                pb.HealOverTime(AmountWithLevel(resource.healAmount,level));
            }
        }
        if(resource.damageAmount > 0){
            if(resource.DamageBehavior == PotionBuilder.PotionType.Instant){
                pb.TakeDamageInstant(resource.healAmount);
            }else{
                pb.TakeDamageOverTime(resource.healAmount);
            }
        }

        if(resource.AffectOtherResistance && resource.resistanceAmount > 0){
            Element element;
            if(resource.resistanceElement == ElementsEnum.Fire) element = new FireElement();
            else if(resource.resistanceElement == ElementsEnum.Water) element = new WaterElement();
            else element = new RockElement();

            pb.Resistence(element, resource.resistanceAmount);
        }else{
            if(resource.resistanceAmount > 0){
                pb.Resistence(resource.resistanceElement, resource.resistanceAmount);
            }
            if(resource.weaknessAmount > 0){
                pb.Resistence(resource.WeakElement, resource.weaknessAmount);
            }
        }

        if(resource.speedAmount > 0){
            pb.Speed(resource.speedAmount);
        }

        return pb.Build();
    }

    public static int AmountWithLevel(int amount,int level){
        return amount + (int)Math.Round((float)(amount/5) * (level-1));
    }
    public static float AmountWithLevel(float amount,int level){
        return (float)(amount + Math.Round((amount/5) * (level-1)));
    }

    public static ItemData GetItemData(byte id){
        if(itemDB.ContainsKey(id)){
            return itemDB[id];
        }else{
            return null;
        }
    }
}

public enum ItemType{
    Potion,
    Equipament,
    Seed,
    Ingredient
}

public class ItemData{
    public byte level = 1;
    public string name {get;} = "";
    public string description {get;} = "";
    public Texture2D iconFile {get;}
    public Half price {get;} = (Half)1;
    public byte stackMaxSize {get;} = 5;
    public ItemType type {get;} = ItemType.Potion;
    public PotionEffect effect {get;}
    public Equipament equipament {get;}

    public ItemData(string name, string description, Texture2D iconFile, Half price, byte stackMaxSize, ItemType type, PotionEffect effect, Equipament equipament){
        this.name = name;
        this.description = description;
        this.iconFile = iconFile;
        this.price = price;
        this.stackMaxSize = stackMaxSize;
        this.type = type;
        this.effect = effect;
        this.equipament = equipament;
    }
}
