using System;
using System.Collections.Generic;
using Godot;

public static class PlantingSystem{
    static Dictionary<string, PlantResource> plantDB = new();

    /// <summary>
    /// Sets up the plant system by loading all the plants in the <c>res://Resources/Plants/</c> directory.
    /// </summary>
    /// <remarks>
    /// This function is called by the GameManager when it is initialized.
    /// It loads all the plants in the <c>res://Resources/Plants/</c> directory and adds them to the plant database.
    /// If a plant is already in the database, it is skipped.
    /// If a plant has a null growth process, result, or result quantity of 0 or less, it is skipped.
    /// </remarks>
    public static void SetupPlantSystem(){
        const string path = "res://Resources/Plants/";
        DirAccess dir = DirAccess.Open(path);
        if(dir == null) return;
        dir.ListDirBegin();
        string fileName;
        while((fileName = dir.GetNext()) != ""){
            PlantResource plant = GD.Load<PlantResource>(path + "/" + fileName);

            fileName = fileName.Replace(".tres","");
            if(plant == null) continue;
            if(
                plant.GrowthProcess == null || 
                plant.result == null ||
                plant.resultQuantity <= 0 ||
                plantDB.ContainsKey(fileName)
            ) continue;

            plant.name = fileName;

            plantDB.Add(fileName, plant);
        }
    }
    public static PlantResource GetPlant(string name){
        if(!plantDB.ContainsKey(name)) return null;
        return plantDB[name];
    }
}

public class PlantZoneData{
    public byte Columns {get;}
    public byte Rows {get;}
    public SoilTileData[] SoilsData;
    public Action WhenUpdate;

    /// <summary>
    /// Creates a new plant zone data.
    /// </summary>
    /// <param name="columns">The number of columns in the plant zone.</param>
    /// <param name="rows">The number of rows in the plant zone.</param>
    /// <param name="soilsLife">An array of bytes, where each value is the life of a soil tile in the plant zone. The length of the array is the number of columns times the number of rows.</param>
    public PlantZoneData(byte columns, byte rows,byte[] soilsLife){
        this.Columns = columns;
        this.Rows = rows;
        SoilsData = new SoilTileData[columns * rows];
        for(int i = 0; i < soilsLife.Length; i++){
            SoilsData[i] = new SoilTileData(
                (byte)i,
                soilsLife[i],
                null
            );
        }
    }

    /// <summary>
    /// Adds a plant to the specified position in the plant zone.
    /// </summary>
    /// <param name="fileName">The name of the plant resource to add.</param>
    /// <param name="position">The position index in the SoilsData array where the plant should be added.</param>
    /// <param name="progress">The initial growth progress of the plant.</param>
    /// <returns>Returns true if the plant was successfully added; otherwise, false.</returns>
    public bool Add(string fileName, int position, short progress){
        PlantData plantData = new(PlantingSystem.GetPlant(fileName),progress);
        return SoilsData[position].SetPlant(plantData.plant);
    }

    /// <summary>
    /// Updates the plant zone. This will invoke the WhenUpdate event and then update all the soil tiles in the plant zone.
    /// </summary>
    public void Update(){
        WhenUpdate.Invoke();
        if(SoilsData == null) return;
        foreach(SoilTileData soil in SoilsData){
            if(soil == null) continue;
            soil.Update();
        }
    }
    public SoilTileData this[int i] => SoilsData[i];
}
public class SoilTileData{
    public byte position = 0;
    public byte soilLife = 100;
    public byte skippedSeconds = 0;
    public PlantData plantData;
    public Action<PlantData> WhenPlantSet;

    public ItemResource GetPlantResult() => plantData?.plant?.result;
    public PlantResource GetPlant() => plantData?.plant;
    public byte GetPlantResultQuantity() => plantData?.plant?.resultQuantity ?? 0;
    public Texture2D DeadTexture() => plantData?.plant?.DeadPlant;
    public Texture2D[] GrowthProcess() => plantData?.plant?.GrowthProcess;
    public short GetProgress() => plantData?.progress ?? 0;
    public short GrowthDuration() => plantData?.plant?.growthDurationSeconds ?? 0;
    public bool IsDead() => plantData?.isDead ?? false;
    public ItemResource GetSeed() => plantData?.plant?.seed ?? null;

    
    /// <summary>
    /// Creates a new SoilTileData instance.
    /// </summary>
    /// <param name="position">The position index in the SoilsData array where the soil tile is located.</param>
    /// <param name="soilLife">The initial soil life of the soil tile.</param>
    /// <param name="plant">The plant resource that should be grown in the soil tile. If null, the soil tile will not have a plant.</param>
    /// <param name="progress">The initial growth progress of the plant. Defaults to 0.</param>
    public SoilTileData(byte position, byte soilLife,PlantResource plant, short progress = 0){
        this.position = position;
        this.soilLife = soilLife;
        WhenPlantSet += (plantData) => {};
        
        if(plant == null) return;
        plantData = new(plant,progress);
    }

    
    /// <summary>
    /// Updates the soil tile. This will decrement the soil life once every 36 seconds if the plant is not null.
    /// If the plant is not null, its growth progress will be incremented.
    /// </summary>
    public void Update(){
        if(soilLife == 0) return;
        if(plantData != null){
            plantData.Update();
        }

        skippedSeconds++;
        if(skippedSeconds <= 36 || plantData == null) return;

        skippedSeconds = 0;
        soilLife--;
    }
    
    /// <summary>
    /// Sets the plant in the soil tile to the given plant resource.
    /// If the given plant resource is null or the soil tile already has a plant, returns false.
    /// </summary>
    /// <param name="plant">The plant resource to set.</param>
    /// <returns>Returns true if the plant was successfully set; otherwise, false.</returns>
    public bool SetPlant(PlantResource plant){
        if(plant == null || plantData != null) return false;
        plantData = new(plant,0);
        if(WhenPlantSet != null)
            WhenPlantSet.Invoke(plantData);
        return true;
    }
    /// <summary>
    /// Removes the current plant from the soil tile, if any.
    /// </summary>
    /// <returns>Returns true if a plant was successfully removed; otherwise, false.</returns>
    public bool RemovePlant(){
        if(plantData == null) return false;
        plantData = null;
        return true;
    }

    public bool ContainsPlant() => plantData != null;
    
    /// <summary>
    /// Adds the specified amount of life to the soil tile.
    /// </summary>
    /// <param name="amount">The amount of soil life to add.</param>
    /// <returns>Returns true if the soil life was successfully added; otherwise, false if it exceeds the maximum allowed value of 100.</returns>
    public bool AddSoilLife(byte amount){
        if(soilLife + amount > 100) return false;
        soilLife += amount;
        return true;
    }
}

public class PlantData{
    public PlantResource plant;
    public short progress = 0;
    public bool isDead = false;
    public PlantData(PlantResource plant, short progress){
        this.plant = plant;
        this.progress = progress;
    }
    public void Update(){
        progress++;
    }
}