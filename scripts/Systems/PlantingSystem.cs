using System;
using System.Collections.Generic;
using Godot;

public static class PlantingSystem{
    static Dictionary<string, PlantResource> plantDB = new();

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
                // plant.seed == null || 
                // plant.result == null ||
                plantDB.ContainsKey(fileName)
            ) continue;

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

    public bool Add(string fileName, int position, short progress){
        PlantData plantData = new(PlantingSystem.GetPlant(fileName),progress);
        return SoilsData[position].SetPlant(plantData.plant);
    }

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

    public SoilTileData(byte position, byte soilLife,PlantResource plant, short progress = 0){
        this.position = position;
        this.soilLife = soilLife;
        WhenPlantSet += (plantData) => {};
        
        if(plant == null) return;
        plantData = new(plant,progress);
    }
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
    public bool SetPlant(PlantResource plant){
        if(plant == null || plantData != null) return false;
        plantData = new(plant,0);
        if(WhenPlantSet == null) return false;
        WhenPlantSet.Invoke(plantData);
        return true;
    }

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