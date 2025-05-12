using Godot;
using System;
using System.Linq;
public static class SaveData{

    public static string saveFilePath = "res://Data/saveData/save.txt";

    /// <summary>
    /// Loads a save file and sets up the Player with the values from the save file.
    /// </summary>
    /// <param name="player">The player to set up.</param>
    public static void LoadSaveFile(Player player){
        if(saveFilePath == "") return;
        
        FileAccess file = FileAccess.Open(SaveData.saveFilePath, FileAccess.ModeFlags.Read);
        string[] lines = file.GetAsText().Replace("\r","").Split('\n');

        file.Close();

        player.lifeSystem = new(byte.Parse(lines[0]),10);
        player.handItemIndex = byte.Parse(lines[1]);

        string[] inventoryItems = lines[2].Split('|');
        foreach(string item in inventoryItems){
            if(item == "") continue;
            string[] itemData = item.Split(';');
            byte id = byte.Parse(itemData[0]);
            byte quantity = byte.Parse(itemData[1]);

            player.inventory.Add(id,quantity);
        }

        string[] equipaments = lines[3].Split('|');
        foreach(string equipament in equipaments){
            if(equipament == "") continue;
            byte id = byte.Parse(equipament);
            player.equipamentSys.AddEquipament(id);
        }

        string[] soilsLifesRaw = lines[4].Split(';');
        byte[] soilsLifes = new byte[soilsLifesRaw.Length-1];
        for(int i=0;i<soilsLifes.Length;i++){
            if(soilsLifesRaw[i].Trim() == "") continue;
            soilsLifes[i] = byte.Parse(soilsLifesRaw[i]);
        }
        player.plantZoneData = new PlantZoneData(3,3,soilsLifes);
        if(player.PlantZone != null) player.PlantZone.Setup();

        string[] plants = lines[5].Split('|');
        foreach(string plant in plants){
            if(plant == "") continue;
            string[] itemData = plant.Split(';');
            string name = itemData[0];
            int position = int.Parse(itemData[1]);
            short progress = short.Parse(itemData[2]);
            player.plantZoneData.Add(name,position,progress);
        }
        string[] playerPositionRaw = lines[6].Split('|');
        Vector2 newPosition = new(int.Parse(playerPositionRaw[0]),int.Parse(playerPositionRaw[1]));
        player.GlobalPosition = newPosition;

        string[] checkpoints = lines[7].Split('|');
        player.UpdateKnowsCheckPoints(checkpoints);

        string[] recepies = lines[8].Split('|');
        CraftingSystem.DiscoverMultiples(recepies);

    }

    /// <summary>
    /// Saves the current state of the player to a file.
    /// </summary>
    /// <param name="player">The player whose state is to be saved.</param>
    /// <param name="pos">The current position of the player in the game world.</param>
    /// <remarks>
    /// This function writes the player's life, inventory, equipaments, soil life, planted crops, 
    /// position, known checkpoints, and discovered recipes to the save file. The data is serialized 
    /// in a specific format for later retrieval.
    /// </remarks>
    public static void Save(Player player,Vector2 pos){
        if(saveFilePath == "") return;
        
        FileAccess file = FileAccess.Open(SaveData.saveFilePath, FileAccess.ModeFlags.Write);
        string content = "";

        content += player.lifeSystem.CurrentLife() + "\n";
        content += player.handItemIndex + "\n";

        string inventoryContent = "";
        foreach(ItemData item in player.inventory.items){
            if(item == null) continue;
            inventoryContent += $"{item.id};{item.quantity}|";
        }
        content += inventoryContent + "\n";

        string equipamentsContent = "";
        foreach(ItemResource equipament in player.equipamentSys.equipaments){
            if(equipament == null) continue;
            equipamentsContent += $"{equipament.id}|";
        }
        content += equipamentsContent + "\n";

        string soilsLifesContent = "";
        foreach(SoilTileData soil in player.plantZoneData.SoilsData){
            if(soil == null) continue;
            soilsLifesContent += $"{soil.soilLife};";
        }
        content += soilsLifesContent + "\n";

        string plantsContent = "";

        var soilWithPlants = player.plantZoneData.SoilsData.Where(soil => soil != null && soil.plantData != null);

        foreach(SoilTileData soil in soilWithPlants){
            PlantData plant = soil.plantData;
            PlantResource plantData = plant?.plant;
            plantsContent += $"{plantData.name};{soil.position};{plant.progress}|";
        }
        content += plantsContent + "\n";

        content += $"{Math.Round(pos.X,0)}|{Math.Round(pos.Y,0)}\n";

        content += string.Join('|',player.KnowCheckPoints());

        content += string.Join("|",
                        CraftingSystem.GetRecipes()
                        .Where(x => x.known)
                        .Select(x => x.name)
        );

        file.StoreString(content);


        file.Close();

    }

}

