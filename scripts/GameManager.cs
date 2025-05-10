using System;
using System.Collections.Generic;
using Godot;
public partial class GameManager : Node{

    public static GameManager Instance { get; set; }
    [Export] Player player;
    [Export] Camera camera;

    public Configurator configurator;
    public readonly static byte GAMEUNITS = 16;

    public Pooling pooling;

    public DropItem SpawnItem(Vector2 position, ItemResource item, int quantity){
        return pooling.GrabFroomPool(position, item, quantity);
    }
    public void DespawnItem(DropItem dropItem){
        pooling.PutBackToPool(dropItem);
    } 

    public override void _EnterTree()
    {
        pooling = new ();
        AddChild(pooling);

        ItemDB.SetupItemDB();
        CraftingSystem.SetupRecipes();
        PlantingSystem.SetupPlantSystem();

        Instance = this;
        configurator = new();

        player.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D").ProcessMode = ProcessModeEnum.Always;
        camera.ProcessMode = ProcessModeEnum.Always;
    }

    public static string GUIPath() => "res://assets/GUI/";

    

    public void GameOver(){
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}