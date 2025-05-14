using Godot;
using System;

public partial class NpcDialogZone : Interactable
{
    [Export] DialogResource[] dialogResource;
    [Export] string EventAtEnd;
    public override void _Ready(){
        base._Ready();
        EventHandler.AddEvent(EventAtEnd,()=>{
            player.AddGold(10);
        });
    }
    public override void Interact()
    {
        player.InteractDialog(dialogResource,EventAtEnd);

    }
}
