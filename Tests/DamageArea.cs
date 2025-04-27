using Godot;
using System;

public partial class DamageArea : Area2D{
    [Export] public int damage = 1;
    [Export] public ElementsEnum type = ElementsEnum.Fire; 

    public override void _Ready(){
        BodyEntered += WhenPlayerEnter;
    }

    private void WhenPlayerEnter(Node2D body)
    {
        if(body is Player p) p.Damage(type,damage);
    }


}
