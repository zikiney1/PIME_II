using Godot;
using System;

public partial class PlantingZone : Node2D
{
    [Export] Player player;
    PlantZoneData data;
    [Export(PropertyHint.ArrayType, "Texture2D")] Texture2D[] SoilTextureStates;
    SoilTile[] Soils;

    public override void _EnterTree(){
        data = player.plantZoneData;
        Soils = new SoilTile[data.Columns * data.Rows];
        for(int i = 0; i < Soils.Length; i++){
            Soils[i] = new SoilTile(
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

}

public partial class SoilTile : Sprite2D{
    SoilTileData data;
    Texture2D[] SoilTexture;
    Texture2D[] PlantTexture;
    PlantData plantData;

    public const byte MAXSOILLIFE = 100;
    public Sprite2D PlantSprite;
    public SoilTile(Texture2D[] SoilTexture,SoilTileData data,int x,int y){
        this.data = data;
        this.SoilTexture = SoilTexture;
        Position = new Vector2(x, y);
        
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

        int index = MathM.IndexFromPortion(data.soilLife, SoilTexture.Length, MAXSOILLIFE);
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
            int index = MathM.IndexFromPortion(plantData.progress, PlantTexture.Length, plantData.plant.growthDurationSeconds);
            PlantSprite.Texture = PlantTexture[index];
        }

    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventMouseMotion mouseMotion){
            Vector2 mousePos = GetGlobalMousePosition();
            
            if(GetRect().HasPoint(ToLocal(mousePos))){

            }
        }
    }
}

