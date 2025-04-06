using Godot;
using System;

public partial class Player : CharacterBody2D{
    Vector2S direction;
    float Speed = (64/2)  * 1000;

    public override void _Ready()
    {
        direction = new();
    }

    public override void _PhysicsProcess(double delta){

    }

    public override void _Process(double delta)
    {
        Vector2 direction = Input.GetVector("left", "right", "up", "down");

        if(direction != Vector2.Zero){
            Velocity = direction * Speed * (float)delta;
        }else{
            Velocity = Vector2.Zero;
        }

		MoveAndSlide();

    }


    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion KeyEvent){
            
        }
    }

}
