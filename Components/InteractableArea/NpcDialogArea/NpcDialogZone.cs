using Godot;
using System;

public partial class NpcDialogZone : Interactable
{
    [Export] DialogResource[] dialogResource;
    public override void Interact()
    {
        player.InteractDialog(dialogResource);
    }
}
