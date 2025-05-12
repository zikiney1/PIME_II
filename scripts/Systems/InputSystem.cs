using System.Collections.Generic;
using Godot;

public class InputSystem{

    public enum Actions{
        UsePotion,
        Attack,
        Defend,
        Enter,
        Use,
        Pause
    }
    Dictionary<Actions, string> ActionsNames = new Dictionary<Actions, string>(){
        {Actions.UsePotion, "use_potion"},
        {Actions.Attack, "attack"},
        {Actions.Defend, "defend"},
        {Actions.Enter, "enter"},
        {Actions.Use, "use"},
        {Actions.Pause, "pause"}
    };

    
    /// <summary>
    /// Retrieves a Vector2 representing the directional input from the user.
    /// </summary>
    /// <returns>A Vector2 where the x component corresponds to the horizontal input
    /// (left/right) and the y component corresponds to the vertical input (up/down).
    /// </returns>
    public static Vector2 GetVector(){
        return Input.GetVector("left", "right", "up", "down");
    }
    

}