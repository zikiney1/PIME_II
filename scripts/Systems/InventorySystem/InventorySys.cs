using Godot;
using System;
using System.Collections.Generic;

public class InventorySystem{

    //id = index
    public Dictionary<byte, byte> itemPositions = new();
    public List<ItemData> items = new();
    
    public byte Length => (byte)items.Count;
    public InventorySystem(){
    }

    public bool Add(ItemResource item, byte quantity = 1){
        if(item == null) return false;
        if(itemPositions.ContainsKey(item.id)){
            items[itemPositions[item.id]].quantity += quantity;
            return true;
        }else{
            ItemData data = new(){
                resource = item,
                quantity = quantity
            };

            items.Add(data);
            itemPositions.Add(item.id, (byte)(items.Count-1));
            return false;
        }
    }
    public bool Add(byte id, byte quantity = 1) => Add(ItemDB.GetItemData(id),quantity);
    public bool Remove(byte id, byte quantity = 1){
        if(itemPositions.ContainsKey(id)){
            byte index = itemPositions[id];
            items[index].quantity -= quantity;
            if(items[index].quantity <= 0) {
                items.RemoveAt(index);
                itemPositions.Remove(id);
            }
            return true;
        }else{
            return false;
        }
    }

    public bool Contains(byte id, int quantity = 1) => itemPositions.ContainsKey(id) && items[itemPositions[id]].quantity > 0;    
    
    public ItemData this[byte index] => items[index];

}



public class ItemData{
    public byte id => resource.id;
    public string name => resource.name;
    public Texture2D icon => resource.icon;
    public ItemResource resource;
    public int quantity = 1;
}