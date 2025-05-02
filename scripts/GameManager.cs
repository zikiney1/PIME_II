using System;
using Godot;
public partial class GameManager : Node{

    public static GameManager Instance { get; set; }
    [Export] Player player;
    [Export] Camera camera;

    public Configurator configurator;
    public readonly static byte GAMEUNITS = 16;

    public override void _EnterTree()
    {
        ItemDB.SetupItemDB();
        CraftingSystem.SetupRecipes();
        Instance = this;

        // player = GetNode<Player>("Player");
        // camera = GetNode<Camera>("Camera");

        configurator = new();

        player.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").ProcessMode = ProcessModeEnum.Always;
        camera.ProcessMode = ProcessModeEnum.Always;
    }

    public static string GUIPath() => "res://assets/GUI/";

    public void GameOver(){
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}