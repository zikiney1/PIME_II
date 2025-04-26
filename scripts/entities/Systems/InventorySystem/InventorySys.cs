using Godot;
using System;
using System.Collections.Generic;

public class InventorySystem{
    public byte[] items;
    public InventorySystem(byte slotQuantity){
        items = new byte[slotQuantity];
    }

    public bool Add(byte id, byte position){
        if(items[position] != 0 || id == 0 || position > items.Length-1) return false;
        
        items[position] = id;
        return true;
    }

    public bool Remove(byte position){
        items[position] = 0;
        return true;
    }

    public ItemData this[byte index] => ItemDB.GetItemData(items[index]);
}


public static class ItemDB{
    public static Dictionary<byte, ItemData> itemDB = new();

    public static void SetupItemDB(){
        
    }

    public static ItemData GetItemData(byte id){
        if(itemDB.ContainsKey(id)){
            return itemDB[id];
        }else{
            throw new Exception("Item Data Does not Exist");
        }
    }
}

public enum ItemType{
    Potion,
    Equipament,
    Seed,
    Plant
}

public class ItemData{
    public string name {get;} = "";
    public string description {get;} = "";
    public string iconFile {get;} = "";
    public Half price {get;} = (Half)1;
    public byte stackMaxSize {get;} = 5;
    public ItemType type {get;} = ItemType.Potion;
    public IPotionEffect effect {get;}

    public ItemData(string name, string description, string iconFile, Half price, byte stackMaxSize, ItemType type, IPotionEffect effect){
        this.name = name;
        this.description = description;
        this.iconFile = iconFile;
        this.price = price;
        this.stackMaxSize = stackMaxSize;
        this.type = type;
        this.effect = effect;
    }
}


