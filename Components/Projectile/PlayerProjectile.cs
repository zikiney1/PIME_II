using Godot;


public partial class PlayerProjectile : Projectile
{
    public override void _Ready(){
        base._Ready();
        sprite.Texture = GD.Load<Texture2D>("res://assets/Sprites/test/player_projectile.png");
    }
    public override void WhenEnterBody(Node2D body){
        
    }

    public override void DeSpawn(){
        base.DeSpawn();
        pooling.ReturnPlayerProjectile(this);
    }
}