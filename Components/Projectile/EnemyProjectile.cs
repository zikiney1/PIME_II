using Godot;


public partial class EnemyProjectile : Projectile
{
    public override void WhenEnterBody(Node2D body){
        if(body is Player p){
            p.Damage(damage);
        }

        this.DeSpawn();
    }
    public override void DeSpawn(){
        base.DeSpawn();
        pooling.ReturnEnemyProjectile(this);
    }
}