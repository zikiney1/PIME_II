using Godot;
using System;

public partial class NpcDialogZone : Interactable
{
    [Export(PropertyHint.File)] public string dialogPath;
    [Export] public string EventAtEnd;
    public override void Interact()
    {
        if(!Visible) return;
        player.InteractDialog(dialogPath,EventAtEnd);

    }
}
