using System;
using System.Collections.Generic;
using Godot;
public partial class GameManager : Node{

    public readonly static byte GAMEUNITS = 16;
    public readonly static byte SOILTILESIZE = 3;

    public readonly static uint PlayerBulletMask = 12;
    public readonly static uint EnemyBulletMask = 9;

    public static GameManager Instance { get; set; }
    Player player;
    [Export] Camera camera;
    Random rnd;

    public enum GameState
    {
        Running,
        Menu,
        Paused,
        GameOver
    }

    public Configurator configurator;
    public GameState gameState = GameState.Running;

    public Pooling pooling;

    public Projectile GetBullet(uint maskLayer , Vector2 position, Vector2 direction) =>  pooling.GetBullet(maskLayer,position,direction);

    public DropItem SpawnItem(Vector2 position, ItemResource item, int quantity){
        return pooling.GrabItem(position, item, quantity);
    }
    
    public Coin SpawnCoin(Vector2 position) => pooling.SpawnCoin(position);
    public void DespawnItem(DropItem dropItem)
    {
        pooling.ReturnItem(dropItem);
    } 

    public override void _EnterTree()
    {
        Instance = this;
        pooling = new ();
        AddChild(pooling);

        configurator = new();
        ItemDB.SetupItemDB();
        CraftingSystem.SetupRecipes();
        PlantingSystem.SetupPlantSystem();


    }

    public override void _Ready()
    {

        base._Ready();
        rnd = new ();
        player = Player.Instance;
        player.GetNode<AudioStreamPlayer2D>("Audio/AudioStreamPlayer2D").ProcessMode = ProcessModeEnum.Always;
        camera.ProcessMode = ProcessModeEnum.Always;
    }

    public void SpawnCoins(Vector2 position, int amount, float waitTime = 0.01f, int randomRange = 3)
    {
        if (amount <= 0) return;

        int coinsToSpawn = GetReducedCoinCount(amount);
        if (coinsToSpawn <= 1)
        {
            SpawnCoin(position).quantity = (byte)amount;
            return;
        }

        int baseValue = amount / coinsToSpawn;
        int remainder = amount % coinsToSpawn;
        int current = 0;

        Timer SpawnTimer = new();
        SpawnTimer.WaitTime = waitTime;
        SpawnTimer.Timeout += () => {
            if (current >= coinsToSpawn)
            {
                SpawnTimer.Stop();
                SpawnTimer.QueueFree();
                return;
            }

            Vector2 newPos = new Vector2(
                position.X + rnd.Next(-randomRange, randomRange),
                position.Y + rnd.Next(-randomRange, randomRange)
            );

            var c = SpawnCoin(newPos);

            // First 'remainder' coins get +1
            c.quantity = (byte)(baseValue + (current < remainder ? 1 : 0));

            current++;
            SpawnTimer.Start();
        };

        AddChild(SpawnTimer);
        SpawnTimer.Start();
    }

    private int GetReducedCoinCount(int amount)
    {
        if (amount <= 5) return amount;
        if (amount <= 10) return 4;
        if (amount <= 30) return 6;
        if (amount <= 45) return 7;
        if (amount <= 60) return 8;
        return 10;
    }


    public void SpawnItems(Vector2 position, ItemResource item, int amount=1,float waitTime = 0.01f,int randomRange = 3)
    {
        int current = 0;
        if(amount <= 0) return;
        if (amount == 1)
        {
            SpawnItem(position,item,1);
            return;
        }
        Timer SpawnTimer = new();
        SpawnTimer.WaitTime = waitTime;
        SpawnTimer.Timeout += () => {
            current++;
            if (current >= amount)
            {
                SpawnTimer.Stop();
                SpawnTimer.QueueFree();
                return;
            }
            Vector2 newPos = new Vector2(position.X + rnd.Next(-randomRange , randomRange), position.Y + rnd.Next(-randomRange , randomRange ));
            SpawnCoin(newPos);
            SpawnTimer.Start();
            
        };
        AddChild(SpawnTimer);
        SpawnTimer.Start();
    }


    public static string GUIPath() => "res://assets/GUI/";

    

    public void GameOver(){
        GetTree().ChangeSceneToFile("res://Scenes/GameOver.tscn");
    }
}