using Godot;
using System;

public partial class DropItem : Area2D
{
    public ItemResource item;
    public Pooling polling;
    public int quantity = 1;
    const float amplitude = 4.0f;
    const float frequency = 6.0f;

    private float time = 0.0f;
    private Vector2 startPosition;

    Sprite2D sprite;

    
    
    /// <summary>
    /// Called when the node enters the scene tree for the first time.
    /// </summary>
    /// <remarks>
    /// Sets the scale of the node to 0.5, creates a new sprite and adds it as a child, and creates a new collision shape and adds it as a child.
    /// </remarks>
    public override void _Ready(){
        Scale = new Vector2(0.5f,0.5f);
        sprite = new();
        AddChild(sprite);

        CollisionShape2D collision = new(){
            Shape = new RectangleShape2D(){
                Size = new Vector2(16,16)
            }
        };

        AddChild(collision);
    }

    
    /// <summary>
    /// Spawns the drop item at the given position and with the given item and quantity.
    /// When a player enters the area, the item is added to the player's inventory and the drop item is returned to the pool.
    /// </summary>
    /// <param name="position">The position to spawn the drop item at.</param>
    /// <param name="item">The item to spawn the drop item with.</param>
    /// <param name="quantity">The quantity of the item to spawn the drop item with.</param>
    public void SpawnItem(Vector2 position,ItemResource item, int quantity){
        this.GlobalPosition = position;
        startPosition = this.Position;
        this.item = item;
        this.quantity = quantity;
        sprite.Texture = item.icon;

        BodyEntered += (body) => {
            if(body is Player p){
                p.Add(item, (byte)quantity);
                polling.PutBackToPool(this);
            }
        };
    }

    

    public override void _Process(double delta)
    {
        time += (float)delta;
        float yOffset = Mathf.Sin(time * frequency) * amplitude;
        Position = new Vector2(startPosition.X, startPosition.Y + yOffset);
    }
    
}
