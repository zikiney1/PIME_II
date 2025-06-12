using Godot;
using System;
using System.Collections.Generic;
public static class ItemDB{
    public static Dictionary<byte, ItemResource> itemDB = new();
    static bool activated = false;

    /// <summary>
    /// Sets up the item database by loading all the items in the <c>res://Resources/Items/</c> directory.
    /// </summary>
    /// <remarks>
    /// This function is called by the GameManager when it is initialized.
    /// It loads all the items in the <c>res://Resources/Items/</c> directory and adds them to the item database.
    /// If an item is already in the database, it is skipped.
    /// If an item has a potion effect, the potion effect is scaled up by the item's level.
    /// If an item has an equipament data, the stack max size is set to 1.
    /// If an item has a plant data, the seed is set to the item itself.
    /// </remarks>
    public static void SetupItemDB()
    {
        if(activated) return;
        const string path = "res://Resources/Items/";
        DirAccess dir = DirAccess.Open(path);
        if (dir == null) return;
        dir.ListDirBegin();
        string fileName;
        while ((fileName = dir.GetNext()) != "")
        {
            ItemResource item = GD.Load<ItemResource>(path + "/" + fileName);

            if (itemDB.ContainsKey(item.id)) continue;

            int level = item.PotionEffect == null ? 1 : item.PotionEffect.useLevel ? item.level : 1;


            item.effect = GetPotionEffect(item.PotionEffect, level);
            if (item.equipamentData != null)
            {
                if (item.stackMaxSize != 1) item.stackMaxSize = 1;
            }

            if (item.plantData != null) item.plantData.seed = item;

            itemDB.Add(item.id, item);
        }
        activated = true;
    }
    
    /// <summary>
    /// Creates a PotionEffect from a PotionEffectResource, scaled up by the given level.
    /// </summary>
    /// <param name="resource">The PotionEffectResource to create the PotionEffect from.</param>
    /// <param name="level">The level to scale the PotionEffect by. Defaults to 1.</param>
    /// <returns>The created PotionEffect.</returns>
    public static PotionEffect GetPotionEffect(PotionEffectResource resource,int level = 1){
        if (resource == null) return null;

        PotionBuilder pb = new (resource.duration);
        if(resource.healAmount > 0){
            if(resource.HealBehavior == PotionBuilder.PotionType.Instant){
                pb.HealInstant(AmountWithLevel(resource.healAmount,level));
            }else{
                pb.HealOverTime(AmountWithLevel(resource.healAmount,level));
            }
        }
        if(resource.takeDamageAmount > 0){
            if(resource.TakeDamageBehavior == PotionBuilder.PotionType.Instant){
                pb.TakeDamageInstant(resource.takeDamageAmount);
            }else if(resource.TakeDamageBehavior == PotionBuilder.PotionType.Periodic){
                pb.TakeDamageOverTime(resource.takeDamageAmount);
            }else{
                pb.TakeDamageAtEnd(resource.takeDamageAmount);
            }
        }

        if(resource.damageAmount > 0){
            pb.IncreaseDamage(AmountWithLevel(resource.damageAmount,level));
        }

        if (resource.defenseAmount > 0)
        {
            pb.Resistence(resource.defenseAmount);
        }

        if(resource.speedAmount > 0){
            pb.Speed(resource.speedAmount);
        }

        return pb.Build();
    }

    /// <summary>
    /// Returns the amount modified by the level.
    /// The returned value is the amount plus the amount divided by 5, times the level minus one, rounded to the nearest integer.
    /// </summary>
    /// <param name="amount">The amount to modify.</param>
    /// <param name="level">The level to use for the modification.</param>
    /// <returns>The modified amount.</returns>
    public static int AmountWithLevel(int amount,int level){
        return amount + (int)Math.Round((float)(amount/5) * (level-1));
    }
    public static float AmountWithLevel(float amount,int level){
        return (float)(amount + Math.Round((amount/5) * (level-1)));
    }

    public static ItemResource GetItemData(byte id){
        if(itemDB.ContainsKey(id)){
            return itemDB[id];
        }else{
            return null;
        }
    }
    public static EquipamentData GetEquipamentData(byte id){
        if(itemDB.ContainsKey(id)){
            return itemDB[id].equipamentData;
        }else{
            return null;
        }
    }
}

public enum ItemType
{
    Potion,
    Equipament,
    Seed,
    Ingredient,
    Resource,
    Mission
}
