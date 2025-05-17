using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pooling : Node2D
{
    List<DropItem> poolItems = new();
    List<DropItem> activeItems = new();
    List<PlayerProjectile> playerProjectilesActive = new();
    List<PlayerProjectile> playerProjectilesDeactive = new();
    List<EnemyProjectile> enemyProjectilesActive = new();
    List<EnemyProjectile> enemyProjectilesDeactive = new();

    Random rnd = new();
    public override void _Ready()
    {
        base._Ready();
        for (int i = 0; i < 20; i++){
            PlayerProjectile ppj = new();
            ppj.pooling = this;
            ppj.DeActivate();
            
            playerProjectilesDeactive.Add(ppj);
            this.AddChild(ppj);
        }

        for (int i = 0; i < 20; i++){
            EnemyProjectile epj = new();
            epj.pooling = this;
            epj.DeActivate();
            
            enemyProjectilesDeactive.Add(epj);
            this.AddChild(epj);
        }
    }

    public PlayerProjectile GetPlayerProjectile(Vector2 position, Vector2 direction){
        PlayerProjectile ppj;

        if(playerProjectilesDeactive.Count > 0){
            ppj = playerProjectilesDeactive.Last();
            playerProjectilesDeactive.RemoveAt(playerProjectilesDeactive.Count - 1);
            ppj.Activate();
        }else{
            ppj = new();
            ppj.pooling = this;
            this.AddChild(ppj);
        }
        ppj.direction = direction;
        ppj.GlobalPosition = position;
        ppj.Rotation = (float)(direction.Angle() + (Math.PI / 2));
        playerProjectilesActive.Add(ppj);
        return ppj;
    }
    public void ReturnPlayerProjectile(PlayerProjectile ppj){
        ppj.DeActivate();
        playerProjectilesActive.Remove(ppj);
        playerProjectilesDeactive.Add(ppj);
    }

    public EnemyProjectile GetEnemyProjectile(Vector2 position, Vector2 direction){
        EnemyProjectile epj;

        if(enemyProjectilesDeactive.Count > 0){
            epj = enemyProjectilesDeactive.Last();
            enemyProjectilesDeactive.RemoveAt(enemyProjectilesDeactive.Count - 1);
            epj.Activate();
        }else{
            epj = new();
            epj.pooling = this;
            this.AddChild(epj);
        }
        epj.direction = direction;
        epj.GlobalPosition = position;
        epj.Rotation = (float)(direction.Angle() + (Math.PI / 2));
        enemyProjectilesActive.Add(epj);
        return epj;
    }
    public void ReturnEnemyProjectile(EnemyProjectile epj){
        epj.DeActivate();
        enemyProjectilesActive.Remove(epj);
        enemyProjectilesDeactive.Add(epj);
    }

    /// <summary>
    /// Grabs a DropItem from the pool, or creates a new one if the pool is empty.
    /// The DropItem is then spawned at the given position, and the given item and quantity are set on the DropItem.
    /// </summary>
    /// <param name="position">The position to spawn the DropItem at.</param>
    /// <param name="item">The item to spawn the DropItem with.</param>
    /// <param name="quantity">The quantity of the item to spawn the DropItem with.</param>
    /// <returns>The spawned DropItem.</returns>
    public DropItem GrabItem(Vector2 position, ItemResource item, int quantity){
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
    public void ReturnItem(DropItem dropItem){
        activeItems.Remove(dropItem);
        poolItems.Add(dropItem);
        dropItem.SetPhysicsProcess(false);
        dropItem.SetProcess(false);

        dropItem.Visible = false;
    }
}
