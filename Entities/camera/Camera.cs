using Godot;
using System;

public partial class Camera : Camera2D
{
    [Export] Player player;
    private Vector2 roomSize;
    private Vector2 currentRoom;
    private Vector2 targetPosition;
    private bool isMoving = false;

    [Export] private float moveDuration = 1f; // duração da transição
    private float moveTimer = 0f;
    private Vector2 moveStart;

    public override void _Ready()
    {
        roomSize = GetViewportRect().Size + (GetViewportRect().Size/4);
        currentRoom = GetRoom(player.GlobalPosition);
        GlobalPosition = currentRoom * roomSize + roomSize / 2f;

        ProcessMode = ProcessModeEnum.Always; // <- Importante! Permite que a câmera funcione mesmo com o jogo pausado
        player.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").ProcessMode = ProcessModeEnum.Always;
    }


    public override void _Process(double delta)
    {
        if (isMoving)
        {
            moveTimer += (float)delta;
            float t = Mathf.Clamp(moveTimer / moveDuration, 0, 1);
            GlobalPosition = moveStart.Lerp(targetPosition, t);

            if (t >= 1.0f)
            {
                FinishCameraMove();
            }
            return;
        }

        Vector2 playerRoom = GetRoom(player.GlobalPosition);
        if (playerRoom != currentRoom)
        {
            StartCameraMove(playerRoom);
        }
    }

    private void StartCameraMove(Vector2 newRoom)
    {
        currentRoom = newRoom;
        moveStart = GlobalPosition;
        targetPosition = currentRoom * roomSize + roomSize / 2f;
        moveTimer = 0f;
        isMoving = true;

        GetTree().Paused = true; // Pausa tudo (menos a câmera)
    }

    private void FinishCameraMove()
    {
        GlobalPosition = targetPosition;
        isMoving = false;
        GetTree().Paused = false; // Retoma o jogo
    }

    private Vector2 GetRoom(Vector2 position)
    {
        return new Vector2(
            Mathf.Floor(position.X / roomSize.X),
            Mathf.Floor(position.Y / roomSize.Y)
        );
    }
}
