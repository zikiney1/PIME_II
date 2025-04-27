using Godot;
using System;
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

    public bool ContainsItemInQuantity(Item item, byte quantity){
        return ContainsItem(item) && item.quantity >= quantity;
    }

    public bool Remove(byte position, byte quantityToRemove=1){
        if(position > items.Length-1) return false;
        if(items[position] == null) return true;
        
        items[position].quantity -= quantityToRemove;
        if(items[position].quantity <= 0 )
            items[position] = null;
        return true;
    }

    public bool Remove(Item item, byte quantityToRemove=1){
        if(item == null) return false;
        for(int i = 0; i < items.Length; i++){
            if(items[i].id == item.id){
                items[i].quantity -= quantityToRemove;
                if(items[i].quantity <= 0 )
                    items[i] = null;
                return true;
            }
        }
        return false;
    }

    public Item this[byte index] => items[index];
    public ItemData GetItemData(int position) => ItemDB.GetItemData(items[position].id);
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
