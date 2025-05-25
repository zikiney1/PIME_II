using Godot;
using System;

public partial class Camera : Camera2D
{
    Player player;


    public override void _Ready()
    {
        SetScreenPosition();
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = 6;

        player = Player.Instance;
    }

    public override void _PhysicsProcess(double delta)
    {
        SetScreenPosition();
    }


    /// <summary>
    /// Adjusts the camera's global position to center around the player's current grid-based position,
    /// ensuring smooth transitions by aligning the camera to the calculated grid center based on the player's position.
    /// </summary>
    public void SetScreenPosition()
    {
        if(player == null) player = Player.Instance;
        Vector2 playerPos = player.GlobalPosition;
        Vector2 ScreenSize = GetViewportRect().Size / (Zoom * 1.1f);

        float x = (float)(Math.Floor(playerPos.X / ScreenSize.X) * ScreenSize.X + ScreenSize.X / 2);
        float y = (float)(Math.Floor(playerPos.Y / ScreenSize.Y) * ScreenSize.Y + ScreenSize.Y / 2);
        GlobalPosition = new Vector2(x, y);
    }

}
