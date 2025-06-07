using Godot;
using System;

public partial class CraftZone : Interactable
{
    public override void _Ready()
    {
        base._Ready();
        GetNode<AnimationPlayer>("AnimationPlayer").Play("animation");
    }
    public override void Interact()
    {
        player.InteractCraft();
    }

}
