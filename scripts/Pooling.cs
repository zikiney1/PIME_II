using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pooling : Node2D
{
    List<DropItem> poolItems = new();
    List<DropItem> activeItems = new();
    Random rnd = new();
    public Pooling(){

    }

    /// <summary>
    /// Grabs a DropItem from the pool, or creates a new one if the pool is empty.
    /// The DropItem is then spawned at the given position, and the given item and quantity are set on the DropItem.
    /// </summary>
    /// <param name="position">The position to spawn the DropItem at.</param>
    /// <param name="item">The item to spawn the DropItem with.</param>
    /// <param name="quantity">The quantity of the item to spawn the DropItem with.</param>
    /// <returns>The spawned DropItem.</returns>
    public DropItem GrabFroomPool(Vector2 position, ItemResource item, int quantity){
        DropItem dropItem;

        position += new Vector2((float)rnd.NextDouble(), (float)rnd.NextDouble() + 0.5f);

        if(poolItems.Count > 0){
            dropItem = poolItems.Last();
            poolItems.RemoveAt(poolItems.Count - 1);
        }else{
            dropItem = new();
            dropItem.polling = this;
            this.AddChild(dropItem);
        }
        activeItems.Add(dropItem);
        dropItem.SpawnItem(position, item, quantity);

        dropItem.Visible = true;
        dropItem.SetPhysicsProcess(true);
        dropItem.SetProcess(true);

        return dropItem;
    }

    /// <summary>
    /// Returns a DropItem back to the pool, disabling its processing and visibility.
    /// </summary>
    /// <param name="dropItem">The DropItem to return to the pool.</param>
    public void PutBackToPool(DropItem dropItem){
        activeItems.Remove(dropItem);
        poolItems.Add(dropItem);
        dropItem.SetPhysicsProcess(false);
        dropItem.SetProcess(false);

        dropItem.Visible = false;
    }
}
