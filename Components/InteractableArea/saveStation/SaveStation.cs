using Godot;
using System;

public partial class SaveStation : Interactable
{
    [Export] Node2D SavePosition;
    [Export] Vector2 savePos;

    public override void _Ready(){
        base._Ready();
        if(SavePosition == null){
            GD.PushError("SavePosition is null");
            Node2D possible = GetNode<Node2D>("Node2D");
            if(possible != null) SavePosition = possible;
            else savePos = this.GlobalPosition * 1.5f;

        }else{
            savePos = SavePosition.GlobalPosition;
        }
    }
    public override void Interact()
    {
        player.Save(savePos);
    }
}
