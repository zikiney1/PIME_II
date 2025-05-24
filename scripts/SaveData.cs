using Godot;
using System;
using System.Linq;
using System.Text;
using System.Threading;
public static class SaveData{

    public static string saveFilePath = "res://Data/saveData/save.txt";

    /// <summary>
    /// Loads a save file and sets up the Player with the values from the save file.
    /// </summary>
    /// <param name="player">The player to set up.</param>
    public static void LoadSaveFile(Player player)
    {
        if (saveFilePath == "") return;
        int indexer = 0;

        FileAccess file = FileAccess.Open(SaveData.saveFilePath, FileAccess.ModeFlags.Read);
        string[] lines = file.GetAsText().Replace("\r", "").Split('\n');

        file.Close();

        //atualiza as informacoes do jogador
        string[] pInfo = lines[indexer].Split('|');
        player.lifeSystem = new(byte.Parse(pInfo[0]), 10);
        player.handItemIndex = byte.Parse(pInfo[1]);
        player.gold = int.Parse(pInfo[2]);
        if (pInfo.Length > 3){
            if(pInfo[3] != "") player.equipamentSys.AddEquipament(byte.Parse(pInfo[3]));
        }

        indexer++;
        //atualiza o inventario
        if (lines[indexer] != "")
        {
            string[] inventoryItems = lines[indexer].Split('|');
            foreach (string item in inventoryItems)
            {
                if (item == "") continue;
                string[] itemData = item.Split(';');
                byte id = byte.Parse(itemData[0]);
                byte quantity = byte.Parse(itemData[1]);

                player.Add(id, quantity);
            }
        }

        indexer++;
        //atualiza a vida dos terrenos
        if (lines[indexer] != "")
        {
            string[] soilsLifesRaw = lines[indexer].Split(';');
            byte[] soilsLifes = new byte[soilsLifesRaw.Length - 1];
            for (int i = 0; i < soilsLifes.Length; i++)
            {
                if (soilsLifesRaw[i].Trim() == "") continue;
                soilsLifes[i] = byte.Parse(soilsLifesRaw[i]);
            }
            player.plantZoneData = new PlantZoneData(GameManager.SOILTILESIZE, GameManager.SOILTILESIZE, soilsLifes);
        }
        else
        {
            player.plantZoneData = new PlantZoneData(GameManager.SOILTILESIZE, GameManager.SOILTILESIZE, null);
        }
        player.PlantZone.Setup();

        indexer++;
        //atualizar as plantas
        if (lines[indexer] != "")
        {
            string[] plants = lines[indexer].Split('|');
            foreach (string plant in plants)
            {
                if (plant == "") continue;
                string[] itemData = plant.Split(';');
                string name = itemData[0];
                int position = int.Parse(itemData[1]);
                short progress = short.Parse(itemData[2]);
                player.plantZoneData.Add(name, position, progress);
            }
        }

        indexer++;
        //coloca a posição do jogador no checkpoint
        if (lines[indexer] != "")
        {
            string[] playerPositionRaw = lines[indexer].Split('|');
            Vector2 newPosition = new(int.Parse(playerPositionRaw[0]), int.Parse(playerPositionRaw[1]));
            player.GlobalPosition = newPosition;
        }

        //atualizar os warpszones
        if (lines[indexer] != "")
        {
            string[] checkpoints = lines[indexer].Split('|');
            player.UpdateKnowsCheckPoints(checkpoints);
        }

        //atualizar receitas
        if (lines[indexer] != "")
        {
            string[] recepies = lines[indexer].Split('|');
            CraftingSystem.DiscoverMultiples(recepies);
        }

        if (lines.Length > 7)
        {
            GameManager.Instance.LoadQuests(lines[indexer]);
        }

    }

    /// <summary>
    /// Saves the current state of the player to a file.
    /// </summary>
    /// <param name="player">The player whose state is to be saved.</param>
    /// <remarks>
    /// This function writes the player's life, inventory, equipaments, soil life, planted crops, 
    /// position, known checkpoints, and discovered recipes to the save file. The data is serialized 
    /// in a specific format for later retrieval.
    /// </remarks>
    public static void Save(Player player)
    {
        Thread saveThread = new(() =>
        {

            if (saveFilePath == "")
            {
                GD.PushError("Save file path is empty");
                return;
            }

            FileAccess file = FileAccess.Open(SaveData.saveFilePath, FileAccess.ModeFlags.Write);
            StringBuilder sb = new();

            //informação do jogador
            sb.AppendLine(player.lifeSystem.CurrentLife() + "|" + player.handItemIndex + "|" + player.gold + "|" + player.equipamentSys.IdData());


            //inventario
            string inventoryContent = "";
            foreach (ItemData item in player.inventory.items)
            {
                if (item == null) continue;
                inventoryContent += $"{item.id};{item.quantity}|";
            }
            sb.AppendLine(inventoryContent);

            //vida dos terrenos
            string soilsLifesContent = "";
            foreach (SoilTileData soil in player.plantZoneData.SoilsData)
            {
                if (soil == null) continue;
                soilsLifesContent += $"{soil.soilLife};";
            }
            sb.AppendLine(soilsLifesContent);

            //plantas
            string plantsContent = "";
            var soilWithPlants = player.plantZoneData.SoilsData.Where(soil => soil != null && soil.plantData != null);
            foreach (SoilTileData soil in soilWithPlants)
            {
                PlantData plant = soil.plantData;
                PlantResource plantData = plant?.plant;
                plantsContent += $"{plantData.name};{soil.position};{plant.progress}|";
            }
            sb.AppendLine(plantsContent);

            //posição do jogador
            sb.AppendLine($"{Math.Round(player.GlobalPosition.X, 0)}|{Math.Round(player.GlobalPosition.Y, 0)}");

            //checkpoints
            sb.AppendLine(string.Join('|', player.KnowCheckPoints()));

            //receitas
            sb.AppendLine(string.Join("|",
                CraftingSystem.GetRecipes()
                    .Where(x => x.known)
                    .Select(x => x.name)
            ));

            sb.AppendLine(GameManager.Instance.SaveQuests());

            file.StoreString(sb.ToString());
            file.Close();
        });
        
        saveThread.Start();

    }

}

