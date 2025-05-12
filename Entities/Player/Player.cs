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

    
    public PotionModifier potionModifier = new();

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
        inventory = new();
        equipamentSys = new();

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
        Speed = GameManager.GAMEUNITS  * 350;

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

        float speedModifier = equipamentSys.speed + potionModifier.speed +1;
        float speedTotal = (Speed * speedModifier) / (MathM.BoolToInt(Input.IsActionPressed("defend")) + 1 );
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

    /// <summary>
    /// Adds the specified quantity of the given item to the inventory.
    /// If the item being added is the same as the current hand item, updates the item portrait.
    /// </summary>
    /// <param name="item">The item to add to the inventory.</param>
    /// <param name="quantity">The quantity of the item to add. Defaults to 1.</param>
    /// <returns>True if the item was added to an existing stack, otherwise false.</returns>
    public bool Add(ItemResource item,byte quantity = 1){
        bool result = inventory.Add(item,quantity);
        if(HandItem.id == item.id)
            UpdatePortrait();
        return result;
    }
    /// <summary>
    /// Removes the specified quantity of the given item from the inventory.
    /// If the item being removed is the same as the current hand item, updates the item portrait.
    /// </summary>
    /// <param name="item">The item to remove from the inventory.</param>
    /// <param name="quantity">The quantity of the item to remove. Defaults to 1.</param>
    /// <returns>True if the item was successfully removed, otherwise false.</returns>
    public bool Remove(ItemResource item,byte quantity = 1) {
        bool result = inventory.Remove(item.id,quantity);
        if(HandItem.id == item.id)
            UpdatePortrait();
        return result;
    }

    /// <summary>
    /// Updates the portrait of the hand item in the GUI.
    /// </summary>
    void UpdatePortrait(){
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);
        GUI.UpdateHandItem(handItemData.name,handItemData.icon,HandItem.quantity);
    }


    /// <summary>
    /// Changes the hand item to the next item in the inventory, if none are available, sets the hand item to null.
    /// </summary>
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

        /// <summary>
        /// Interacts with the given soil tile using the current hand item.
        /// If the hand item is a seed, attempts to plant it in the soil tile.
        /// If the hand item is not a seed, attempts to use it to fertilize the soil tile.
        /// </summary>
        /// <param name="data">The soil tile data to interact with.</param>
        /// <param name="quantity">The quantity of the hand item to use. Defaults to 1.</param>
    public void InteractWithSoilTile(SoilTileData data,byte quantity = 1){
        if(HandItem == null) return;
        if(ItemDB.GetItemData(HandItem.id).type == ItemType.Seed){
            Plant(data);
        }else{
            Fertilize(data,quantity);
        }
    }

        /// <summary>
        /// Attempts to plant the current hand item as a seed in the given soil tile.
        /// </summary>
        /// <param name="data">The soil tile data to plant the seed in.</param>
        /// <remarks>
        /// Only works if the current hand item is a seed, the soil tile does not already contain a plant,
        /// and the player is allowed to plant.
        /// </remarks>
    void Plant(SoilTileData data){
        ItemResource HandItemResource = UseHandItem(ItemType.Seed);
        if( HandItemResource == null || data.ContainsPlant() || !canPlant) return;
        if(HandItemResource.plantData == null ) return;

        data.SetPlant(HandItemResource.plantData);
    }

    /// <summary>
    /// Attempts to fertilize the given soil tile with the current hand item.
    /// </summary>
    /// <param name="data">The soil tile data to fertilize.</param>
    /// <param name="quantity">The quantity of the hand item to use.</param>
    /// <remarks>
    /// Only works if the current hand item is the fertilizing item, the soil tile is not already fully fertilized,
    /// and the player is allowed to plant.
    /// </remarks>
    void Fertilize(SoilTileData data,byte quantity){
        if(HandItem == null || !canPlant) return;
        if(HandItem.id != 7) return ;

        if(!data.AddSoilLife((byte)(quantity * 3))) return;
        UseHandItem(ItemType.Resource,quantity);
    }

    /// <summary>
    /// Stops planting. Sets the player state to idle.
    /// </summary>
    public void StopPlanting(){
        state = EntitieState.Idle;
    }
    /// <summary>
    /// Teleports the player to the specified position and sets the state to idle.
    /// </summary>
    /// <param name="pos">The target position to teleport the player to.</param>
    public void Teleport(Vector2 pos){
        state = EntitieState.Idle;
        Position = pos;
    }

    /// <summary>
    /// Uses the current hand item as a potion.
    /// If the hand item is a potion, its effect is applied to the player.
    /// </summary>
    void UsePotion(){
        ItemResource handItemData = UseHandItem(ItemType.Potion);
        if(handItemData == null || handItemData.effect == null) return;

        handItemData.effect.Apply(this);
    }

    /// <summary>
    /// Uses the current hand item if it matches the given type and there is enough of it in the inventory.
    /// If the hand item is used, it is removed from the inventory and the player's hand item is updated.
    /// </summary>
    /// <param name="type">The type of item to use.</param>
    /// <param name="quantity">The quantity of the item to use. Defaults to 1.</param>
    /// <returns>The item data of the used item, or null if the item was not used.</returns>
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
    /// <summary>
    /// Executes an attack by setting the hit area position and modifying the attack speed
    /// based on equipment and potion modifiers.
    /// </summary>
    protected override void Attack(){
        HitArea.Position = lastDirection * GameManager.GAMEUNITS;
        AttackSpeed += equipamentSys.attackSpeed + potionModifier.attackSpeed;
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

    public void Damage(int amount){
        float modifier = potionModifier.defense + equipamentSys.defense;
        this.Damage(modifier,amount);
    }

}
