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

    public PlayerProjectile PlayerShoot(Vector2 position, Vector2 direction) =>  pooling.GetPlayerProjectile(position,direction);

    public EnemyProjectile EnemyShoot(Vector2 position, Vector2 direction) => pooling.GetEnemyProjectile(position,direction);

    public DropItem SpawnItem(Vector2 position, ItemResource item, int quantity){
        return pooling.GrabItem(position, item, quantity);
    }
    public void DespawnItem(DropItem dropItem){
        pooling.ReturnItem(dropItem);
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