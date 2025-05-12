using Godot;
using System;

public partial class SaveStation : Interactable
{
    [Export] Node2D SavePosition;
    public Vector2 savePos;

    Node2D SaveScreen;
    public string name;

    public override void _Ready(){
        base._Ready();
        
        name = Name;
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
        player.InteractSaveStation(this);
    }
}
