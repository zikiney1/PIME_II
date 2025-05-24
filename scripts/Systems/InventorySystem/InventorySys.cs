using Godot;
using System;
using System.Collections.Generic;

public class InventorySystem {

    //id = index
    public Dictionary<byte, byte> itemPositions = new();
    public List<ItemData> items = new();

    public byte Length => (byte)items.Count;

    public List<ItemData> HandItems = new();

    public InventorySystem()
    {

    }
    /// <summary>
    /// Adds a certain quantity of an item to the inventory.
    /// If the item already exists in the inventory, its quantity is increased by the given amount.
    /// If the item does not exist, it is added to the inventory and the quantity set to the given amount.
    /// </summary>
    /// <param name="item">The item to add to the inventory.</param>
    /// <param name="quantity">The quantity of the item to add. Defaults to 1.</param>
    /// <returns>True if the item already existed in the inventory, otherwise false.</returns>
    public bool Add(ItemResource item, byte quantity = 1)
    {
        // if(Length > 9) return false;
        if (item == null) return false;
        if (itemPositions.ContainsKey(item.id))
        {
            items[itemPositions[item.id]].quantity += quantity;
            return true;
        }
        else
        {
            ItemData data = new()
            {
                resource = item,
                quantity = quantity
            };
            if (item.type == ItemType.Potion || item.type == ItemType.Seed || item.type == ItemType.Resource)
            {
                HandItems.Add(data);
            }
            items.Add(data);
            itemPositions.Add(item.id, (byte)(items.Count - 1));
            return true;
        }
    }
    public bool Add(byte id, byte quantity = 1) => Add(ItemDB.GetItemData(id), quantity);

    /// <summary>
    /// Removes the specified quantity of an item from the inventory using its ID.
    /// If the item's quantity reaches zero or below, it is removed from the inventory.
    /// </summary>
    /// <param name="id">The ID of the item to remove.</param>
    /// <param name="quantity">The quantity of the item to remove. Defaults to 1.</param>
    /// <returns>True if the item was successfully removed or its quantity was decreased, otherwise false.</returns>
    public bool Remove(byte id, byte quantity = 1) {
        if (itemPositions.ContainsKey(id)) {
            byte index = itemPositions[id];
            items[index].quantity -= quantity;
            if (items[index].quantity <= 0) {
                ItemResource itr = items[index].resource;
                if (itr.type == ItemType.Potion || itr.type == ItemType.Seed || itr.type == ItemType.Resource) {
                    HandItems.Remove(items[index]);
                }
                items.RemoveAt(index);
                itemPositions.Remove(id);
            }
            return true;
        } else {
            return false;
        }
    }

    public bool Contains(byte id, int quantity = 1) => itemPositions.ContainsKey(id) && items[itemPositions[id]].quantity > 0;
    public bool Contains(ItemResource item, int quantity = 1) => Contains(item.id, quantity);

    public int GetPosition(ItemResource item) => itemPositions[item.id];
    public ItemResource GetItemByID(byte id) => itemPositions.ContainsKey(id) ? items[itemPositions[id]].resource : null;

    public bool IsHandItem(int index)
    {
        if (index < HandItems.Count) {
            if (items[index] == null) return false;
            ItemResource item = items[index].resource;
            return item.type == ItemType.Potion || item.type == ItemType.Seed || item.type == ItemType.Resource;
        }
        else
            return false;
    }

    public ItemData this[byte index]
    {
        get
        {
            if (index < items.Count)
                return items[index];
            else
                return null;
        }
    }

}



public class ItemData
{
    public byte id => resource.id;
    public string name => resource.name;
    public Texture2D icon => resource.icon;
    public ItemResource resource;
    public int quantity = 1;
    public bool isHandItem() => resource.type == ItemType.Potion || resource.type == ItemType.Seed || resource.type == ItemType.Resource;
}