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

    bool isActivated = false;
    

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

    public void SpawnItem(Vector2 position,ItemResource item, int quantity){
        this.Position = position;
        startPosition = this.Position;
        this.item = item;
        this.quantity = quantity;
        sprite.Texture = item.icon;
        isActivated = true;
        BodyEntered += (body) => {
            if(body is Player p){
                p.Add(item, (byte)quantity);
                polling.PutBackToPool(this);
                isActivated = false;
            }
        };
    }

    

    public override void _Process(double delta)
    {
        if(!isActivated) return;
        time += (float)delta;
        float yOffset = Mathf.Sin(time * frequency) * amplitude;
        Position = new Vector2(startPosition.X, startPosition.Y + yOffset);
    }
    
}
