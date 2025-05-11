using Godot;
using System;

public partial class PlantingZone : Node2D
{
    [Export] public Player player;
    public float playerRange = 3f;
    [Export]public Texture2D SelectRegion;
    PlantZoneData data;
    [Export(PropertyHint.ArrayType, "Texture2D")] Texture2D[] SoilTextureStates;
    SoilTile[] Soils;

    [Export] public GameManager gameManager;

    public override void _EnterTree(){
    }
    
    public void Setup(){
        data = player.plantZoneData;
        Soils = new SoilTile[data.Columns * data.Rows];
        for(int i = 0; i < Soils.Length; i++){
            Soils[i] = new SoilTile(
                this,
                SoilTextureStates,
                player.plantZoneData[i],
                (i % data.Rows) * GameManager.GAMEUNITS,
                (i / data.Columns) * GameManager.GAMEUNITS
            );
            AddChild(Soils[i]);
            player.plantZoneData[i].WhenPlantSet += Soils[i].SetPlant;
        }
        data.WhenUpdate += Update;
    }



    public void Update(){
        foreach(SoilTile soil in Soils){
            soil.UpdateTexture();
            soil.UpdatePlant();
        }
    }
    public override void _Process(double delta)
    {
        if(!MathM.IsInRange(player.GlobalPosition, GlobalPosition, GameManager.GAMEUNITS * data.Columns)){
            foreach (SoilTile soil in Soils) soil.SelectSprite.Visible = false;
            return;
        }
            
        

        foreach (SoilTile soil in Soils)
        {
            Vector2 SoilMiddle = soil.GlobalPosition + new Vector2(GameManager.GAMEUNITS / 2, GameManager.GAMEUNITS / 2);
            Vector2 PlayerMiddle = player.GlobalPosition + new Vector2(GameManager.GAMEUNITS / 2, GameManager.GAMEUNITS / 2);

            soil.isInRange = MathM.IsInRange(PlayerMiddle, SoilMiddle, 32);

            if(soil.isInRange && soil.IsHoovering()) soil.SelectSprite.Visible = true;
            else soil.SelectSprite.Visible = false;

        }
    }

    
}

public partial class SoilTile : Sprite2D{
    public SoilTileData data;
    Texture2D[] SoilTexture;
    Texture2D[] PlantTexture;
    PlantData plantData => data.plantData;

    PlantingZone Father;
    Player player;

    public const byte MAXSOILLIFE = 100;
    public Sprite2D PlantSprite;
    public Sprite2D SelectSprite;
    public bool isInRange = false;

    public SoilTile(PlantingZone Father ,Texture2D[] SoilTexture,SoilTileData data,int x,int y){
        this.Father = Father;
        player = Father.player;
        this.data = data;
        this.SoilTexture = SoilTexture;
        Position = new Vector2(x, y);

        SelectSprite = new();
        SelectSprite.Position = Vector2.Zero;
        SelectSprite.Visible = false;
        SelectSprite.Texture = Father.SelectRegion;
        AddChild(SelectSprite);
        
        UpdateTexture();
    }

    public void SetPlant(PlantData plantData){
        if(plantData == null) return;
        if(plantData.plant == null) return;

        data.plantData = plantData;
        PlantTexture = data.GrowthProcess();
        
        if(PlantSprite == null){
            PlantSprite = new();
            PlantSprite.Position = Vector2.Zero;
            AddChild(PlantSprite);
        }else{
            PlantSprite.Texture = null;
        }
        UpdatePlant();
    }


    public void UpdateTexture(){
        if (SoilTexture.Length == 0) return;

        int index = (int)Math.Floor((1f - (data.soilLife / (float)MAXSOILLIFE)) * (SoilTexture.Length - 1));
        index = Math.Clamp(index, 0, SoilTexture.Length - 1);
        Texture = SoilTexture[index];
    }

    public void UpdatePlant(){
        if(plantData == null) return;
        if (PlantTexture.Length == 0 || data.IsDead() || plantData.plant == null) return;            
        

        if(plantData.progress >= data.GrowthDuration() && data.DeadTexture() != null){
            PlantSprite.Texture = data.DeadTexture();
            plantData.isDead = true;
        }else{
            double ratio = plantData.progress / data.GrowthDuration();
            int index = (int)Math.Floor(ratio * (PlantTexture.Length - 1));
            index = Math.Clamp(index, 0, PlantTexture.Length - 1);
            PlantSprite.Texture = PlantTexture[index];
        }

    }

    public bool IsHoovering(){
        return GetRect().HasPoint(ToLocal(GetGlobalMousePosition()));
    }

    public override void _Input(InputEvent @event)
    {
        if(!isInRange) return;
        if(@event is InputEventMouseButton mouseButton){
            if(mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed && IsHoovering()){
                player.InteractWithSoilTile(data);
            }
            else if(mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && IsHoovering() && data.plantData != null){
                CollectPlant();
            }
        }
    }

    void CollectPlant(){
        Father.gameManager.SpawnItem(GlobalPosition, data.GetPlantResult(), data.GetPlantResultQuantity());
        Father.gameManager.SpawnItem(GlobalPosition, data.GetSeed(), 1 );
        data.RemovePlant();
        PlantSprite.Texture = null;
    }
}

