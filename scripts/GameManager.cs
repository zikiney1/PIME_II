using System;
using Godot;
public partial class GameManager : Node{

    public static GameManager Instance { get; set; }
    [Export] Player player;
    [Export] Camera camera;

    public Configurator configurator;
    public readonly static byte GAMEUNITS = 32;

    public override void _Ready(){
        Instance = this;

        configurator = new();

        player.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").ProcessMode = ProcessModeEnum.Always;
        camera.ProcessMode = ProcessModeEnum.Always;
    }

    public void GameOver(){
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}