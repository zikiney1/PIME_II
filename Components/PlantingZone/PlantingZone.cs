using Godot;
using System;

public partial class PlantingZone : Area2D
{
    [Export] public Player player;
    [Export] public float playerRange = 3.5f;
    [Export]public Texture2D SelectRegion;
    PlantZoneData data;
    [Export(PropertyHint.ArrayType, "Texture2D")] Texture2D[] SoilTextureStates;
    SoilTile[] Soils;
    public bool isInRange = false;
    private Vector2I lastPlayerTile = new(-1, -1);

    public override void _EnterTree(){
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
        RectangleShape2D shape = new();
        shape.Size = new Vector2(data.Columns * GameManager.GAMEUNITS, data.Rows * GameManager.GAMEUNITS);
        GetNode<CollisionShape2D>("CollisionShape2D").Shape = shape;
    }


    public void Update(){
        foreach(SoilTile soil in Soils){
            soil.UpdateTexture();
            soil.UpdatePlant();
        }
    }
    
    public override void _Process(double delta)
    {
        if (!isInRange) return;

        Vector2 local = (player.GlobalPosition - GlobalPosition) / GameManager.GAMEUNITS;
        Vector2I currentTile = new(Mathf.FloorToInt(local.X), Mathf.FloorToInt(local.Y));

        if (currentTile != lastPlayerTile)
        {
            lastPlayerTile = currentTile;
            SetSoilsInRange();
        }

        foreach (SoilTile soil in Soils)
        {
            soil.SelectSprite.Visible = soil.isInRange && soil.IsHoovering();
        }
    }


    public void SetSoilsInRange()
    {
        int rTiles = Mathf.CeilToInt(playerRange / GameManager.GAMEUNITS);
        Vector2 local = (player.GlobalPosition - GlobalPosition) / GameManager.GAMEUNITS;
        int px = Mathf.FloorToInt(local.X), py = Mathf.FloorToInt(local.Y);

        int minX = Mathf.Clamp(px - rTiles, 0, data.Columns - 1);
        int maxX = Mathf.Clamp(px + rTiles, 0, data.Columns - 1);
        int minY = Mathf.Clamp(py - rTiles, 0, data.Rows - 1);
        int maxY = Mathf.Clamp(py + rTiles, 0, data.Rows - 1);

        // reset
        foreach (SoilTile soil in Soils) soil.isInRange = false;

        // só tiles no retângulo
        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                Soils[y * data.Columns + x].isInRange = true;
    }

}

public partial class SoilTile : Sprite2D{
    public SoilTileData data;
    Texture2D[] SoilTexture;
    Texture2D[] PlantTexture;
    PlantData plantData;

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

        this.plantData = plantData;
        PlantTexture = plantData.plant.GrowthProcess;
        
        PlantSprite = new();
        PlantSprite.Position = Vector2.Zero;
        AddChild(PlantSprite);
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
        if (PlantTexture.Length == 0 || plantData.isDead) return;
        if(plantData.plant == null) return;

        if(plantData.progress >= plantData.plant.growthDurationSeconds && plantData.plant.DeadPlant != null){
            PlantSprite.Texture = plantData.plant.DeadPlant;
            plantData.isDead = true;
        }else{
            double ratio = plantData.progress / (double)plantData.plant.growthDurationSeconds;
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
        }
        if(@event is InputEventMouseMotion mouseMotion){
            if(IsHoovering()){
                SelectSprite.Visible = true;
            }else{
                SelectSprite.Visible = false;
            }
        }
    }
}

