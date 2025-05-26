using Godot;
using System;

public partial class Camera : Camera2D
{
    [Export] Vector2 ScreenSize = new Vector2(480, 320);
    [Export] Vector2 BossPosition;

    Vector2 curScreen = new();

    bool isInBooss = false;

    public static Camera Instance { get; private set; }
    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }


    public override void _Ready()
    {
        TopLevel = true;
        GlobalPosition = GetParent<Node2D>().GlobalPosition;
        UpdateScreen(curScreen);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isInBooss) return;
        Vector2 parentScreen = (GetParent<Node2D>().GlobalPosition / ScreenSize).Floor();
        if (!parentScreen.IsEqualApprox(curScreen))
        {
            UpdateScreen(parentScreen);
        }
    }

    public void UpdateScreen(Vector2 newScreen)
    {
        curScreen = newScreen;
        Position = curScreen * ScreenSize + ScreenSize * 0.5f;
    }

    public void SetToBoss()
    {
        isInBooss = true;
        Position = BossPosition;
        Zoom = new Vector2(2,2);
    }
}
