using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class InventorySystem{
    public Item[] items;
    public InventorySystem(byte slotQuantity){
        items = new Item[slotQuantity];
    }

    public bool Add(Item item, byte position){
        if(items[position] != null|| item == null || position > items.Length-1) return false;
        if(items[position].id == item.id ){
            items[position].AddItemToStack(item.quantity);
        }else{
            items[position] = item;
        }
        return true;
    }

    public bool Add(Item item){
        for(int i = 0; i < items.Length; i++){
            if(items[i].id ==item.id){
                items[i].AddItemToStack(item.quantity);
                return true;
            }
        }
        return false;
    }

    public bool ContainsItem(Item item) => items.Contains(item);

    public bool Remove(byte position, byte quantityToRemove=1){
        if(position > items.Length-1) return false;
        if(items[position] == null) return true;
        
        items[position].quantity -= quantityToRemove;
        if(items[position].quantity <= 0 )
            items[position] = null;
        return true;
    }

    public ItemData this[byte index] => ItemDB.GetItemData(items[index].id);
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
    Ingredient
}

public class ItemData{
    public string name {get;} = "";
    public string description {get;} = "";
    public Texture2D iconFile {get;}
    public Half price {get;} = (Half)1;
    public byte stackMaxSize {get;} = 5;
    public ItemType type {get;} = ItemType.Potion;
    public PotionEffect effect {get;}

    public ItemData(string name, string description, Texture2D iconFile, Half price, byte stackMaxSize, ItemType type, PotionEffect effect){
        this.name = name;
        this.description = description;
        this.iconFile = iconFile;
        this.price = price;
        this.stackMaxSize = stackMaxSize;
        this.type = type;
        this.effect = effect;
    }
}

public class Item{
    public byte id {get;}
    public byte quantity;
    public Item(byte id){
        this.id = id;
    }
    public bool AddItemToStack(byte quantity=1){
        if(quantity < ItemDB.GetItemData(id).stackMaxSize) this.quantity++;
        else return false;
        return true;
    }
}
