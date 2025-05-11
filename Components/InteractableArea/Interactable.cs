using Godot;
using System;

public partial class Interactable : Area2D
{
    public bool isInRange = false;
    [Export] public Player player;

    public override void _Ready()
    {
        base._Ready();
        AreaEntered += (Area2D area) =>{
            if(area.Name == "InteractableRange") isInRange = true;
        };
        AreaExited += (Area2D area) => {
            if(area.Name == "InteractableRange") isInRange = false;
        };
        
    }

    public override void _Input(InputEvent @event)
    {
        if(!isInRange) return;
        if(@event is InputEventKey KeyEvent){
            if(KeyEvent.IsActionPressed("use")) Interact();
        }
    }


    public virtual void Interact(){}
}
