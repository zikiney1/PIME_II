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
    
    /// <summary>
    /// Sets up the planting zone with the given player and plant zone data.
    /// </summary>
    /// <remarks>
    /// This method should be called after the <see cref="Player"/> and <see cref="PlantZoneData"/>
    /// properties have been set.
    /// </remarks>
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
    }



    public void Update(){
        foreach(SoilTile soil in Soils){
            soil.UpdateTexture();
            soil.UpdatePlant();
        }
    }
    
    /// <summary>
    /// Processes the visibility and selection state of soil tiles based on the player's position.
    /// </summary>
    /// <param name="delta">The time elapsed since the last frame, in seconds.</param>
    /// <remarks>
    /// If the player is outside the range of the planting zone, all soil tile selection sprites are hidden.
    /// Otherwise, it calculates the middle points of the player and each soil tile to determine if a soil tile
    /// is in range and being hovered over, updating the visibility of the selection sprite accordingly.
    /// </remarks>
    public override void _Process(double delta)
    {
        if(!MathM.IsInRange(player.GlobalPosition, GlobalPosition, GameManager.GAMEUNITS * data.Columns)){
            foreach (SoilTile soil in Soils) soil.SelectSprite.Visible = false;
            return;
        }

        foreach (SoilTile soil in Soils){
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

    /// <summary>
    /// Creates a new SoilTile.
    /// </summary>
    /// <param name="Father">The PlantingZone this soil tile is part of.</param>
    /// <param name="SoilTexture">An array of textures for the different states of the soil tile.</param>
    /// <param name="data">The data for the soil tile.</param>
    /// <param name="x">The x position of the soil tile.</param>
    /// <param name="y">The y position of the soil tile.</param>
    /// <remarks>
    /// Sets up the soil tile with its position, data, and textures.
    /// </remarks>
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

    /// <summary>
    /// Sets the plant data for the soil tile and initializes the plant textures.
    /// </summary>
    /// <param name="plantData">The plant data to set for the soil tile.</param>
    /// <remarks>
    /// If the plant data or the plant itself is null, the function returns early.
    /// Otherwise, it assigns the plant data, retrieves the growth process textures,
    /// and initializes or clears the plant sprite. Finally, it updates the plant display.
    /// </remarks>
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

    
    /// <summary>
    /// Updates the texture of the soil tile based on its current life.
    /// </summary>
    /// <remarks>
    /// If the soil texture array is empty, the function does nothing.
    /// Otherwise, it calculates the index of the texture to use based on the current soil life,
    /// clamps the index to the valid range, and sets the texture of the soil tile accordingly.
    /// </remarks>
    public void UpdateTexture(){
        if (SoilTexture.Length == 0) return;
        int index = (int)Math.Floor((1f - (data.soilLife / (float)MAXSOILLIFE)) * (SoilTexture.Length - 1));
        index = Math.Clamp(index, 0, SoilTexture.Length - 1);
        Texture = SoilTexture[index];
    }

    
    /// <summary>
    /// Updates the texture of the plant in the soil tile based on its growth progress.
    /// </summary>
    /// <remarks>
    /// If the plant data is null, the function does nothing.
    /// Otherwise, it checks if the plant is dead or its growth process textures are empty.
    /// If the plant is dead and it has a dead texture, it sets the plant sprite's texture to the dead texture and sets the plant as dead.
    /// Otherwise, it calculates the index of the texture to use based on the plant's growth progress,
    /// clamps the index to the valid range, and sets the texture of the plant sprite accordingly.
    /// </remarks>
    public void UpdatePlant(){
        if(plantData == null) return;
        if (PlantTexture.Length == 0 || data.IsDead() || plantData.plant == null) return;            
        

        if(plantData.progress >= data.GrowthDuration() && data.DeadTexture() != null){
            PlantSprite.Texture = data.DeadTexture();
            plantData.isDead = true;
        }else{
            double ratio = (double)plantData.progress / data.GrowthDuration();
            int index = (int)Math.Floor(ratio * (PlantTexture.Length - 1));
            index = Math.Clamp(index, 0, PlantTexture.Length - 1);
            PlantSprite.Texture = PlantTexture[index];
        }

    }

    /// <summary>
    /// Determines if the mouse cursor is currently hovering over the soil tile.
    /// </summary>
    /// <returns>Returns true if the mouse is hovering over the soil tile; otherwise, false.</returns>
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

    /// <summary>
    /// Collects the plant from the soil tile.
    /// </summary>
    /// <remarks>
    /// Spawns the plant result item at the global position of the soil tile with the quantity specified by the plant data.
    /// Spawns the seed item at the global position of the soil tile.
    /// Removes the plant from the soil tile.
    /// Sets the plant sprite's texture to null.
    /// </remarks>
    void CollectPlant(){
        Father.gameManager.SpawnItem(GlobalPosition, data.GetPlantResult(), data.GetPlantResultQuantity());
        Father.gameManager.SpawnItem(GlobalPosition, data.GetSeed(), 1 );
        data.RemovePlant();
        PlantSprite.Texture = null;
    }
}

