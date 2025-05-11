using Godot;
using System;
using System.Linq;

public partial class Player : Entitie{
    Area2D HitArea;
    Area2D InteractableRange;

    Timer StopAttackTimer;
    Timer PlantZoneUpdater;
    Timer PlantTimer;
    Timer PlantCoolDownTimer;
    
    public Action WhenPlantUpdate;
    [Export] PlantingZone PlantZone;

    GameGui GUI;
    CraftingGui CraftGUI;

    bool isDefending = false;
    bool canPlant = true;
    byte handItemIndex = 0;
    public float PlantRange = GameManager.GAMEUNITS * 1.5f;

    public void UpdateHearts() => GUI.UpdateHearts();

    public InventorySystem inventory;
    public EquipamentSys equipamentSys;
    public PlantZoneData plantZoneData;
    // public PlantZoneData plantZoneData = new (3,3,[
    //     100,70,100,
    //     70,20,100,
    //     100,50,60
    // ]);


    public override void _EnterTree()
    {
        Speed = GameManager.GAMEUNITS  * 500;
        entitieModifier = new();
        equipamentSys = new(entitieModifier);

        inventory = new(9);
        // lifeSystem = new(5,10);
        LoadSaveFile();
        plantZoneData.WhenUpdate += PlantZone.Update;


        lifeSystem.WhenDies += Die;

        // inventory.Add(ItemDB.GetItemData(7),10);//adubo

        // inventory.Add(ItemDB.GetItemData(1));
        // inventory.Add(ItemDB.GetItemData(10),1);
        // inventory.Add(ItemDB.GetItemData(2),2);
        // inventory.Add(ItemDB.GetItemData(3),2);

        // equipamentSys.AddEquipament(ItemDB.GetItemData(5).equipamentData);


        WhenPlantUpdate += () =>{
            plantZoneData.Update();
            PlantZoneUpdater.Start();
        };
        PlantZoneUpdater = NodeMisc.GenTimer(this,1f, () =>{
            WhenPlantUpdate?.Invoke();
        });
        PlantZoneUpdater.Start();

        HandItem = inventory[handItemIndex];

    }


    protected override void Ready_(){
        
        HitArea = GetNode<Area2D>("HitArea");
        
        StopAttackTimer = NodeMisc.GenTimer(this, 0.5f, StopAttack);
        PlantTimer = NodeMisc.GenTimer(this,3f, StopPlanting);
        PlantCoolDownTimer = NodeMisc.GenTimer(this,0.5f, ()=> canPlant = true);

        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        CraftGUI.Deactivate();

        // plantZoneData.Add("plant_test",3,0);
        InteractableRange = GetNode<Area2D>("InteractableRange");



        if(HandItem == null) {
            ChangeHandItem();
        }
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);

        GUI.UpdateHandItem(handItemData.name,handItemData.icon,HandItem.quantity);

    }

    public override void _PhysicsProcess(double delta){

    }

    public override void _Process(double delta)
    {
        if(state == EntitieState.Attacking || CraftGUI.Visible || state == EntitieState.Lock) return;
        Vector2 direction = InputSystem.GetVector();

        float speedTotal = ( Speed * entitieModifier.GetSpeedModifier() ) / (MathM.BoolToInt(Input.IsActionPressed("defend")) + 1 );
        Walk(direction, delta,speedTotal);

    }


    public override void _Input(InputEvent @event)
    {
        if(state == EntitieState.Lock) return;
        if(@event is InputEventKey KeyEvent){
            if(KeyEvent.IsActionPressed("defend")) 
                isDefending = !isDefending;
            
            if(KeyEvent.IsActionPressed("attack")) Attack();
            // else if(KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if(KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if(KeyEvent.IsActionPressed("use") && !CraftGUI.Visible) CraftGUI.Activate();
            else if(KeyEvent.IsActionPressed("use") && CraftGUI.Visible) CraftGUI.Deactivate();
            else if(KeyEvent.IsActionPressed("change_item")) ChangeHandItem();


            if(KeyEvent.Keycode == Key.P){
                Save();
            }
        }
    }

    public bool Add(ItemResource item,byte quantity = 1){
        bool result = inventory.Add(item,quantity);
        if(HandItem.id == item.id)
            UpdatePortrait();
        return result;
    }

    void UpdatePortrait(){
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);
        GUI.UpdateHandItem(handItemData.name,handItemData.icon,HandItem.quantity);
    }


    void ChangeHandItem(){
        handItemIndex++;
        if(handItemIndex >= inventory.Length) handItemIndex = 0;
        HandItem = inventory[handItemIndex];

        if(HandItem == null){
            ChangeHandItem();
        }else{
            UpdatePortrait();
        }
        
    }

    public void InteractWithSoilTile(SoilTileData data,byte quantity = 1){
        if(HandItem == null) return;
        if(ItemDB.GetItemData(HandItem.id).type == ItemType.Seed){
            Plant(data);
        }else{
            Fertilize(data,quantity);
        }
    }

    void Plant(SoilTileData data){
        ItemResource HandItemResource = UseHandItem(ItemType.Seed);
        if( HandItemResource == null || data.ContainsPlant() || !canPlant) return;
        if(HandItemResource.plantData == null ) return;

        data.SetPlant(HandItemResource.plantData);
    }

    void Fertilize(SoilTileData data,byte quantity){
        if(HandItem == null || !canPlant) return;
        if(HandItem.id != 7) return ;

        if(!data.AddSoilLife((byte)(quantity * 3))) return;
        UseHandItem(ItemType.Resource,quantity);
    }

    public void StopPlanting(){
        state = EntitieState.Idle;
    }

    void UsePotion(){
        ItemResource handItemData = UseHandItem(ItemType.Potion);
        if(handItemData == null || handItemData.effect == null) return;

        handItemData.effect.Apply(this);
    }

    ItemResource UseHandItem(ItemType type, byte quantity = 1){
        if(HandItem == null || quantity <= 0) return null;
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);

        if(handItemData == null) return null;

        if(handItemData.type != type || !inventory.Contains(HandItem.id,quantity)) return null;
        
        if (!inventory.Remove(HandItem.id,quantity)) return null;
        if(inventory[handItemIndex] == null || HandItem.quantity == 0) HandItem = null;

        if(HandItem != null){
            GUI.UpdateHandItem(handItemData.name,handItemData.icon,HandItem.quantity);
        }else{
            GUI.UpdateHandItem("",null,0);
        }

        return handItemData;
    }




    protected override void Attack(){
        HitArea.Position = lastDirection * GameManager.GAMEUNITS;  
        base.Attack();
    }
    protected override void StopAttack(){
        base.StopAttack();
        HitArea.Position = Vector2.Zero;
    }
    protected override void Die(){
        GD.Print("YOU DIED");
    }
    public override void WhenTakeDamage(){
        UpdateHearts();
    }
    public override void whenHeal(){
        UpdateHearts();
    }


    public void LoadSaveFile(){
        if(SaveData.saveFilePath == "") return;
        
        FileAccess file = FileAccess.Open(SaveData.saveFilePath, FileAccess.ModeFlags.Read);
        string[] lines = file.GetAsText().Replace("\r","").Split('\n');

        file.Close();

        lifeSystem = new(byte.Parse(lines[0]),10);
        handItemIndex = byte.Parse(lines[1]);

        string[] inventoryItems = lines[2].Split('|');
        foreach(string item in inventoryItems){
            if(item == "") continue;
            string[] itemData = item.Split(';');
            byte position = byte.Parse(itemData[0]);
            byte id = byte.Parse(itemData[1]);
            byte quantity = byte.Parse(itemData[2]);

            inventory.Add(position,id,quantity);
        }

        string[] equipaments = lines[3].Split('|');
        foreach(string equipament in equipaments){
            if(equipament == "") continue;
            byte id = byte.Parse(equipament);
            equipamentSys.AddEquipament(ItemDB.GetItemData(id).equipamentData);
        }

        string[] soilsLifesRaw = lines[4].Split(';');
        byte[] soilsLifes = new byte[soilsLifesRaw.Length-1];
        for(int i=0;i<soilsLifes.Length;i++){
            if(soilsLifesRaw[i].Trim() == "") continue;
            soilsLifes[i] = byte.Parse(soilsLifesRaw[i]);
        }
        plantZoneData = new PlantZoneData(3,3,soilsLifes);
        if(PlantZone != null) PlantZone.Setup();

        string[] plants = lines[5].Split('|');
        foreach(string plant in plants){
            if(plant == "") continue;
            string[] itemData = plant.Split(';');
            string name = itemData[0];
            int position = int.Parse(itemData[1]);
            short progress = short.Parse(itemData[2]);
            plantZoneData.Add(name,position,progress);
        }

    }

    public void Save(){
        if(SaveData.saveFilePath == "") return;
        
        FileAccess file = FileAccess.Open(SaveData.saveFilePath, FileAccess.ModeFlags.Write);
        string content = "";

        content += lifeSystem.CurrentLife() + "\n";
        content += handItemIndex + "\n";

        string inventoryContent = "";
        foreach(Slot slot in inventory.items){
            if(slot == null) continue;
            inventoryContent += $"{slot.position};{slot.id};{slot.quantity}|";
        }
        content += inventoryContent + "\n";

        string equipamentsContent = "";
        foreach(EquipamentData equipament in equipamentSys.equipaments){
            if(equipament == null) continue;
            equipamentsContent += $"{equipament.GetId()}|";
        }
        content += equipamentsContent + "\n";

        string soilsLifesContent = "";
        foreach(SoilTileData soil in plantZoneData.SoilsData){
            if(soil == null) continue;
            soilsLifesContent += $"{soil.soilLife};";
        }
        content += soilsLifesContent + "\n";

        string plantsContent = "";

        var soilWithPlants = plantZoneData.SoilsData.Where(soil => soil != null && soil.plantData != null);

        foreach(SoilTileData soil in soilWithPlants){
            PlantData plant = soil.plantData;
            PlantResource plantData = plant?.plant;
            plantsContent += $"{plantData.name};{soil.position};{plant.progress}|";
        }
        content += plantsContent + "\n";


        file.StoreString(content);


        file.Close();

    }



}
