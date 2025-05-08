using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pooling : Node2D
{
    List<DropItem> poolItems = new();
    List<DropItem> activeItems = new();
    public Pooling(){

    }

    public DropItem GrabFroomPool(Vector2 position, ItemResource item, int quantity){
        DropItem dropItem;

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

        return dropItem;
    }

    public void PutBackToPool(DropItem dropItem){
        activeItems.Remove(dropItem);
        poolItems.Add(dropItem);
        dropItem.Visible = false;
    }
}
