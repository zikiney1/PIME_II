using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Pooling : Node2D
{
    List<DropItem> poolItems = new();
    List<DropItem> activeItems = new();

    List<Projectile> ActiveProjectiles = new();
    List<Projectile> DeactiveProjectiles = new();

    List<Coin> ActiveCoins = new();
    List<Coin> DeactiveCoins = new();

    Random rnd = new();

    public override void _Ready()
    {
        base._Ready();
        for (int i = 0; i < 20; i++)
        {
            Projectile p = new();
            p.pooling = this;
            p.DeActivate();

            DeactiveProjectiles.Add(p);
            this.AddChild(p);
        }

        for (int i = 0; i < 20; i++)
        {
            Coin c = new();
            c.pool = this;
            c.DeActivate();

            DeactiveCoins.Add(c);
            this.AddChild(c);
        }

    }
    
    public Coin SpawnCoin(Vector2 position){
        Coin coin;
        if (DeactiveCoins.Count > 0)
        {
            coin = DeactiveCoins.Last();
            DeactiveCoins.RemoveAt(DeactiveCoins.Count - 1);
        }
        else
        {
            coin = new();
            coin.pool = this;
            this.AddChild(coin);
        }
        coin.Activate();
        coin.GlobalPosition = position;
        coin.startPosition = position;
        ActiveCoins.Add(coin);
        return coin;
    }

    public Coin ReturnCoin(Coin coin)
    {
        coin.DeActivate();
        ActiveCoins.Remove(coin);
        DeactiveCoins.Add(coin);
        return coin;
    }

    public Projectile GetBullet(CollisionObject2D shooter, Vector2 position, Vector2 direction)
    {
        Projectile pj;

        if (DeactiveProjectiles.Count > 0)
        {
            pj = DeactiveProjectiles.Last();
            DeactiveProjectiles.RemoveAt(DeactiveProjectiles.Count - 1);
        }
        else
        {
            pj = new();
            pj.pooling = this;
            this.AddChild(pj);
        }
        pj.Activate();
        pj.direction = direction;
        pj.GlobalPosition = position;
        pj.Rotation = (float)(direction.Angle() + (Math.PI / 2));
        pj.CollisionMask = shooter.CollisionMask;
        ActiveProjectiles.Add(pj);
        return pj;
    }

    public void ReturnBullet(Projectile pj){
        pj.DeActivate();
        ActiveProjectiles.Remove(pj);
        DeactiveProjectiles.Add(pj);
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
