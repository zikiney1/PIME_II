using Godot;
using System;
using System.Linq;
using System.Text;
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

        //atualiza as informacoes do jogador
        string[] pInfo = lines[0].Split('|');
        player.lifeSystem = new(byte.Parse(pInfo[0]),10);
        player.handItemIndex = byte.Parse(pInfo[1]);
        player.gold = int.Parse(pInfo[2]);

        //atualiza o inventario
        string[] inventoryItems = lines[1].Split('|');
        foreach(string item in inventoryItems){
            if(item == "") continue;
            string[] itemData = item.Split(';');
            byte id = byte.Parse(itemData[0]);
            byte quantity = byte.Parse(itemData[1]);

            player.Add(id,quantity);
        }

        //atualiza os equipamentos
        string[] equipaments = lines[2].Split('|');
        foreach(string equipament in equipaments){
            if(equipament == "") continue;
            byte id = byte.Parse(equipament);
            player.equipamentSys.AddEquipament(id);
        }

        //atualiza a vida dos terrenos
        string[] soilsLifesRaw = lines[3].Split(';');
        byte[] soilsLifes = new byte[soilsLifesRaw.Length-1];
        for(int i=0;i<soilsLifes.Length;i++){
            if(soilsLifesRaw[i].Trim() == "") continue;
            soilsLifes[i] = byte.Parse(soilsLifesRaw[i]);
        }
        player.plantZoneData = new PlantZoneData(GameManager.SOILTILESIZE,GameManager.SOILTILESIZE,soilsLifes);
        if(player.PlantZone != null) player.PlantZone.Setup();

        //atualizar as plantas
        string[] plants = lines[4].Split('|');
        foreach(string plant in plants){
            if(plant == "") continue;
            string[] itemData = plant.Split(';');
            string name = itemData[0];
            int position = int.Parse(itemData[1]);
            short progress = short.Parse(itemData[2]);
            player.plantZoneData.Add(name,position,progress);
        }

        //coloca a posição do jogador no checkpoint
        string[] playerPositionRaw = lines[5].Split('|');
        Vector2 newPosition = new(int.Parse(playerPositionRaw[0]),int.Parse(playerPositionRaw[1]));
        player.GlobalPosition = newPosition;

        //atualizar os warpszones
        string[] checkpoints = lines[6].Split('|');
        player.UpdateKnowsCheckPoints(checkpoints);

        //atualizar receitas
        string[] recepies = lines[7].Split('|');
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
        StringBuilder sb = new();

        //informação do jogador
        sb.AppendLine(player.lifeSystem.CurrentLife() + "|" + player.handItemIndex + "|" + player.gold);


        //inventario
        string inventoryContent = "";
        foreach(ItemData item in player.inventory.items){
            if(item == null) continue;
            inventoryContent += $"{item.id};{item.quantity}|";
        }
        sb.AppendLine(inventoryContent);

        //equipamentos
        string equipamentsContent = "";
        foreach(ItemResource equipament in player.equipamentSys.equipaments){
            if(equipament == null) continue;
            equipamentsContent += $"{equipament.id}|";
        }
        sb.AppendLine(equipamentsContent);

        //vida dos terrenos
        string soilsLifesContent = "";
        foreach(SoilTileData soil in player.plantZoneData.SoilsData){
            if(soil == null) continue;
            soilsLifesContent += $"{soil.soilLife};";
        }
        sb.AppendLine(soilsLifesContent);

        //plantas
        string plantsContent = "";
        var soilWithPlants = player.plantZoneData.SoilsData.Where(soil => soil != null && soil.plantData != null);
        foreach(SoilTileData soil in soilWithPlants){
            PlantData plant = soil.plantData;
            PlantResource plantData = plant?.plant;
            plantsContent += $"{plantData.name};{soil.position};{plant.progress}|";
        }
        sb.AppendLine(plantsContent);

        //posição do jogador
        sb.AppendLine($"{Math.Round(pos.X, 0)}|{Math.Round(pos.Y, 0)}");

        //checkpoints
        sb.AppendLine(string.Join('|', player.KnowCheckPoints()));

        //receitas
        sb.AppendLine(string.Join("|",
            CraftingSystem.GetRecipes()
                .Where(x => x.known)
                .Select(x => x.name)
        ));

        file.StoreString(sb.ToString());


        file.Close();

    }

}

