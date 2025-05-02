using Godot;
using System;
using System.Collections.Generic;

public class InventorySystem{
    public Slot[] items;
    public Dictionary<byte, SlotData> itemsDic;
    public InventorySystem(byte slotQuantity){
        items = new Slot[slotQuantity];
        itemsDic = new();
    }

    public bool Add(ItemResource item, byte quantity = 1){
        if(items.Length == itemsDic.Count || quantity == 0) return false;
        
        SlotData slotData = new(quantity);
        for(int i = 0; i < items.Length; i++){
            if(quantity <= 0) break;
            if(items[i] != null && items[i].id == item.id){
                byte quantityToFit;
                if(items[i].quantity + quantity > item.stackMaxSize){
                    quantityToFit = item.stackMaxSize;
                    quantity -= quantityToFit;
                }else{
                    quantityToFit = quantity;
                }
                if(quantityToFit <= 0) break;
                items[i].quantity += quantityToFit;
                slotData.totalQuantity += quantityToFit;
                quantity -= quantityToFit;
            }else if(items[i] == null){
                byte quantityToFit;
                if(quantity > item.stackMaxSize){
                    quantityToFit = item.stackMaxSize;
                    quantity -= item.stackMaxSize;
                }else{
                    quantityToFit = quantity;
                }
                if(quantityToFit <= 0) break;
                items[i] = new Slot(item.id,(byte)i,quantityToFit);
                slotData.positions.Add(items[i]);
                quantity -= quantityToFit;
            }            
        }
        
        if(itemsDic.ContainsKey(item.id)){
            itemsDic[item.id].positions.AddRange(slotData.positions);
            itemsDic[item.id].totalQuantity += slotData.totalQuantity;
        }else{
            itemsDic.Add(item.id, slotData);
        }

        return true;
    }

    public bool Remove(byte id, byte quantity = 1){
        if (!itemsDic.ContainsKey(id) || quantity == 0)
            return false;

        var positions = itemsDic[id].positions;

        for (int i = positions.Count - 1; i >= 0; i--){
            Slot slot = positions[i];
            items[slot.position].quantity -= quantity;
            itemsDic[id].totalQuantity -= quantity;

            if (items[slot.position].quantity <= 0)
            {
                positions.RemoveAt(i);
                items[slot.position] = null;
            }
        }

        return true;
    }
    public bool Remove (int position, byte quantity = 1){
        if(items[position] == null || quantity == 0) return false;
        items[position].quantity -= quantity;
        itemsDic[items[position].id].totalQuantity -= quantity;
        if(items[position].quantity <= 0){
            items[position] = null;
            itemsDic[items[position].id].positions.Remove(items[position]);
        }
        return true;
    }
    public bool Remove(ItemResource item, byte quantity = 1) => Remove(item.id, quantity);

    public bool Contains(byte id, byte quantity = 1) => itemsDic.ContainsKey(id) && itemsDic[id].totalQuantity >= quantity;

    public Slot this[byte index] => items[index];
    public ItemResource GetItemData(int position) => ItemDB.GetItemData(items[position].id);
}


public class Slot{
    public byte id {get;}
    public byte quantity = 1;
    public byte position = 0;
    public Slot(byte id,byte position, byte quantity = 1){
        this.id = id;
        this.quantity = quantity;
        this.position = position;
    }
}

public class SlotData{
    public List<Slot> positions;
    public byte totalQuantity=0;
    public SlotData(byte totalQuantity){
        positions = new();
        this.totalQuantity = totalQuantity;
    }
}