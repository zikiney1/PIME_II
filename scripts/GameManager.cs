using System;
using System.Collections.Generic;
using Godot;
public partial class GameManager : Node{

    public readonly static byte GAMEUNITS = 32;
    public readonly static byte SOILTILESIZE = 3;

    public readonly static uint PlayerBulletMask = 28;
    public readonly static uint EnemyBulletMask = 9;

    public static GameManager Instance { get; set; }
    Player player;
    [Export] Camera camera;
    Random rnd;

    [Export] public NpcDialogZone Apoena;
    [Export] public NpcDialogZone Apua;
    [Export] public NpcDialogZone Karai_dialog;
    [Export] public NpcDialogZone Luna;
    [Export] public NpcDialogZone caua;
    [Export] public NpcDialogZone thauan;
    [Export] public NpcDialogZone moacir;
    [Export] public NpcDialogZone espirito;
    [Export] public Merchant Karai_Merchant;
    [Export] public Node moacirQuestEnemies;
    [Export] public StaticBody2D BossEntrance;

    public string SaveQuests()
    {
        string saveDataLine = "";
        saveDataLine += Apoena.dialogPath + "|";
        saveDataLine += Apua.dialogPath + "|";
        saveDataLine += (Karai_dialog.Visible ? Karai_dialog.dialogPath : "") + "|";
        saveDataLine += Luna.dialogPath + "|";
        saveDataLine += caua.dialogPath + "|";
        saveDataLine += thauan.dialogPath + "|";
        saveDataLine += (moacir.Visible ? moacir.dialogPath : "") + "|";
        saveDataLine += espirito.dialogPath;
        return saveDataLine;
    }

    public void LoadQuests(string saveDataLine)
    {
        if (saveDataLine == "") return;
        string[] saveData = saveDataLine.Split('|');
        Apoena.dialogPath = saveData[0];
        Apua.dialogPath = saveData[1];

        if (saveData[2] == "")
        {
            Karai_dialog.Visible = false;
            Karai_Merchant.Visible = true;
        }
        else
        {
            Karai_dialog.Visible = true;
            Karai_Merchant.Visible = false;
            Karai_dialog.dialogPath = saveData[2];
        }
        Luna.dialogPath = saveData[3];
        caua.dialogPath = saveData[4];
        thauan.dialogPath = saveData[5];

        if (saveData[6] == "")
        {
            moacir.Visible = false;
        }
        else
        {
            moacir.Visible = true;
            moacir.dialogPath = saveData[6];
        }

        espirito.dialogPath = saveData[7];
    }

    public enum GameState
    {
        Running,
        Menu,
        Paused,
        GameOver
    }

    public Configurator config;
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
        pooling = new();
        AddChild(pooling);

        config = new();
        ItemDB.SetupItemDB();
        CraftingSystem.SetupRecipes();
        PlantingSystem.SetupPlantSystem();
        EventHandler.Setup();
        DialogManager.Setup();


    }

    public override void _Ready()
    {
        base._Ready();
        rnd = new();
        player = Player.Instance;
        camera.ProcessMode = ProcessModeEnum.Always;
        AudioSetup();
    }

    void AudioSetup()
    {
        int MasterBusIndex = AudioServer.GetBusIndex("Master");
        int SFXBusIndex = AudioServer.GetBusIndex("SFX");
        int MusicBusIndex = AudioServer.GetBusIndex("Musica");

        float MasterValue = float.Parse(config.Audio["MASTER"]);
        float SFXValue = float.Parse(config.Audio["SFX"]);
        float MusicValue = float.Parse(config.Audio["MUSICA"]);

        AudioServer.SetBusVolumeDb(SFXBusIndex, Mathf.LinearToDb(SFXValue));
        AudioServer.SetBusVolumeDb(MusicBusIndex, Mathf.LinearToDb(MusicValue));
        AudioServer.SetBusVolumeDb(MasterBusIndex, Mathf.LinearToDb(MasterValue));
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
        SpawnTimer.Timeout += () =>
        {
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