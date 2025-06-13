using Godot;
using System;

public partial class NpcDialogZone : Interactable
{
    [Export(PropertyHint.File)] public string dialogPath = "";
    [Export] public string EventAtEnd = "";
    

    public override void _Ready()
    {
        base._Ready();
        if (HasNode("AnimationPlayer"))
        {
            AnimationPlayer pl = GetNode<AnimationPlayer>("AnimationPlayer");
            if (pl.HasAnimation("animation"))
                pl.Play("animation");
        }
    }

    public override void Interact()
    {
        if (!Visible || dialogPath == "" || dialogPath == null) return;
        if (EventAtEnd == null) EventAtEnd = "";
        player.InteractDialog(dialogPath, EventAtEnd);

    }
}
