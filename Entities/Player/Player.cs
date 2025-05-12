using Godot;
using System;
using System.Collections.Generic;

public partial class Player : Entitie{
    [Export] public PlantingZone PlantZone;
    [Export] public SaveStationManager checkPointManager;
    Area2D HitArea;
    Area2D InteractableRange;

    Timer StopAttackTimer;
    Timer PlantZoneUpdater;
    Timer PlantTimer;
    Timer PlantCoolDownTimer;
    
    public Action WhenPlantUpdate;

    GameGui GUI;
    CraftingGui CraftGUI;
    ShopGui ShopGUI;
    SaveStationGui saveGUI;

    bool isDefending = false;
    bool canPlant = true;
    public byte handItemIndex = 0;
    protected ItemData HandItem = null;
    public float PlantRange = GameManager.GAMEUNITS * 1.5f;


    public InventorySystem inventory;
    public EquipamentSys equipamentSys;
    public PlantZoneData plantZoneData;

    public void Save(Vector2 pos) => SaveData.Save(this,pos);
    public void UpdateHearts() => GUI.UpdateHearts();
    public void AddStation(SaveStation saveStation) => saveGUI.AddStation(saveStation);
    public void UpdateKnowsCheckPoints(string[] names) => checkPointManager.UpdateKnows(names);
    public string[] KnowCheckPoints() => saveGUI.ToNames();

    public float gold = 100;

    public override void _EnterTree()
    {
        entitieModifier = new();
        inventory = new();
        equipamentSys = new(entitieModifier);

        HitArea = GetNode<Area2D>("HitArea");
        
        StopAttackTimer = NodeMisc.GenTimer(this, 0.5f, StopAttack);
        PlantTimer = NodeMisc.GenTimer(this,3f, StopPlanting);
        PlantCoolDownTimer = NodeMisc.GenTimer(this,0.5f, ()=> canPlant = true);

        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        ShopGUI = GetNode<ShopGui>("Canvas/ShopGUI");
        saveGUI = GetNode<SaveStationGui>("Canvas/SaveStationGUI");
        InteractableRange = GetNode<Area2D>("InteractableRange");

    }


    protected override void Ready_(){
        Speed = GameManager.GAMEUNITS  * 500;

        SaveData.LoadSaveFile(this);
        plantZoneData.WhenUpdate += PlantZone.Update;
        GUI.Setup();

        lifeSystem.WhenDies += Die;

        WhenPlantUpdate += () =>{
            plantZoneData.Update();
            PlantZoneUpdater.Start();
        };
        PlantZoneUpdater = NodeMisc.GenTimer(this,1f, () =>{
            WhenPlantUpdate?.Invoke();
        });
        PlantZoneUpdater.Start();

        HandItem = inventory[handItemIndex];
        
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
            else if(KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if(KeyEvent.IsActionPressed("change_item")) ChangeHandItem();

        }
    }

    public void InteractMerchant(ItemResource[] shopItems){
        if(ShopGUI.Visible) {
            ShopGUI.Deactivate();
            state = EntitieState.Idle;
        }
        else {
            ShopGUI.Activate(shopItems);
            state = EntitieState.Lock;
        }
    }

    public void InteractCraft(){
        if(CraftGUI.Visible) {
            CraftGUI.Deactivate();
            state = EntitieState.Idle;
        }
        else {
            CraftGUI.Activate();
            state = EntitieState.Lock;
        }

    }

    public void InteractSaveStation(SaveStation saveStation){
        if(saveGUI.Visible) {
            saveGUI.Deactivate();
            state = EntitieState.Idle;
        }
        else {
            saveGUI.Activate(saveStation);
            state = EntitieState.Lock;
        }
    }

    public bool Add(ItemResource item,byte quantity = 1){
        bool result = inventory.Add(item,quantity);
        if(HandItem.id == item.id)
            UpdatePortrait();
        return result;
    }
    public bool Remove(ItemResource item,byte quantity = 1) {
        bool result = inventory.Remove(item.id,quantity);
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
    public void Teleport(Vector2 pos){
        state = EntitieState.Idle;
        Position = pos;
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

}
