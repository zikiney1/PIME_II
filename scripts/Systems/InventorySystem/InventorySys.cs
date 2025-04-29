using Godot;
using System;

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
            items[position].position = position;
        }
        return true;
    }

    public bool Add(Item item){
        if(ContainsItem(item)){
            for(int i = 0; i < items.Length; i++){
                if(items[i].id ==item.id){
                    bool sucessful = items[i].AddItemToStack(item.quantity);
                    if(sucessful){
                        items[i].position = (byte)i;
                        return true;
                    }
                }
            }
        }else{
            for(int i = 0; i < items.Length; i++){
                if(items[i] == null){
                    items[i] = item;
                    items[i].position = (byte)i;
                    return true;
                }
            }
        }
        return false;
    }

    public bool ContainsItem(Item item) => items[item.position] != null && items[item.position].id == item.id;

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

        if(items[item.position] != null && items[item.position].id == item.id){
            items[item.position].quantity -= quantityToRemove;
            if(items[item.position].quantity <= 0 )
                items[item.position] = null;
            return true;
        }
        return false;
    }

    public Item this[byte index] => items[index];
    public ItemResource GetItemData(int position) => ItemDB.GetItemData(items[position].id);
}


public class Item{
    public byte id {get;}
    public byte quantity = 1;
    public byte position = 1;
    public Item(byte id,byte quantity = 1){
        this.id = id;
        this.quantity = quantity;
    }
    public bool AddItemToStack(byte quantity=1){
        if(quantity < ItemDB.GetItemData(id).stackMaxSize) this.quantity++;
        else return false;
        return true;
    }
}
