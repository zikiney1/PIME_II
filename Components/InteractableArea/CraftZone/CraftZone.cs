using Godot;
using System;

public partial class CraftZone : Interactable
{
    public override void Interact()
    {
        player.InteractCraft();
    }

}
