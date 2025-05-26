using Godot;
using System;

public partial class EventTrigger : Area2D
{
    [Export] string eventToTrigger;
    [Export] string whenExitTrigger;
    [Export] bool triggerOnce = true;

    public override void _Ready()
    {
        base._Ready();
        BodyEntered += (body) =>
        {
            if (body is Player)
            {
                EventHandler.EmitEvent(eventToTrigger);
                if (triggerOnce) QueueFree();
            }
        };
        BodyExited += (body) =>
        {
            if (body is Player)
            {
                EventHandler.EmitEvent(whenExitTrigger);
            }
        };
    }
}
