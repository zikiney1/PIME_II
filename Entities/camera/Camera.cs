using Godot;
using System;

public partial class Camera : Camera2D
{
    [Export] Player player;
    Control UIContainer;

    private Vector2 roomSize;
    private Vector2 currentRoom;
    private Vector2 targetPosition;
    private bool isMoving = false;

    [Export] private float moveDuration = 1f; // duração da transição
    private float moveTimer = 0f;
    private Vector2 moveStart;
    Vector2 OffSet = new Vector2(-10, 130);
    Vector2 newZoom;

    public override void _Ready(){
        newZoom = new(1f/ Zoom.X, 1f/ Zoom.Y);
        UIContainer = player.GetNode<Control>("Canvas/GameGUI/GUIContainer");
        OffSet = new(0,UIContainer.GetRect().Size.Y* 8);
        
        roomSize = (GetViewportRect().Size - OffSet) * newZoom;

        currentRoom = GetRoom(player.GlobalPosition);
        GlobalPosition = currentRoom * RoomSize() + RoomSize() / 2f - OffSet;
    }

    Vector2 RoomSize() => (GetViewportRect().Size - OffSet) * newZoom;


    public override void _Process(double delta)
    {
        SetScreenPos((float)delta);
    }

    void SetScreenPos(float delta = 0){
        if (isMoving)
        {
            moveTimer += delta;
            float t = Mathf.Clamp(moveTimer / moveDuration, 0, 1);
            GlobalPosition = moveStart.Lerp(targetPosition, t);

            if (t >= 1.0f)
                FinishCameraMove();
            return;
        }

        Vector2 playerRoom = GetRoom(player.GlobalPosition);
        if (playerRoom != currentRoom)
            StartCameraMove(playerRoom);

    }

    private void StartCameraMove(Vector2 newRoom){
        currentRoom = newRoom;
        moveStart = GlobalPosition;
        targetPosition = currentRoom * RoomSize() + RoomSize() / 2f - OffSet;
        moveTimer = 0f;
        isMoving = true;
        GetTree().Paused = true;
    }

    private void FinishCameraMove(){
        isMoving = false;
        GetTree().Paused = false;
    }

    private Vector2 GetRoom(Vector2 position){
        return new Vector2(
            Mathf.Floor(position.X / RoomSize().X),
            Mathf.Floor(position.Y / RoomSize().Y)
        );
    }
    
}
