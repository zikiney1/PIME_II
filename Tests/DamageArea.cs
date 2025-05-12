using Godot;
using System;

public partial class DamageArea : Area2D{
    [Export] public int damage = 2;

    public override void _Ready(){
        BodyEntered += WhenPlayerEnter;
    }

    private void WhenPlayerEnter(Node2D body)
    {
        if(body is Player p) p.Damage(damage);
    }


}
