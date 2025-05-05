using Godot;
using System;

public partial class Player : Entitie{
    Area2D HitArea;
    public Area2D PlantRange;
    Area2D InteractableRange;

    Timer StopAttackTimer;
    Timer PlantZoneUpdater;
    Timer PlantTimer;
    Timer PlantCoolDownTimer;
    
    public Action WhenPlantUpdate;


    GameGui GUI;
    CraftingGui CraftGUI;

    bool isDefending = false;
    bool canPlant = true;
    byte handItemIndex = 0;

    public void UpdateHearts() => GUI.UpdateHearts();

    public InventorySystem inventory;
    public EquipamentSys equipamentSys;
    public PlantZoneData plantZoneData = new (3,3,[
        100,70,100,
        70,20,100,
        100,50,60
    ]);


    public override void _EnterTree()
    {
        Speed = GameManager.GAMEUNITS  * 500;
        entitieModifier = new();
        equipamentSys = new(entitieModifier);
        inventory = new(9);
        lifeSystem = new(entitieModifier, 5,10);
        lifeSystem.WhenDies += Die;

        inventory.Add(ItemDB.GetItemData(7),10);//adubo

        inventory.Add(ItemDB.GetItemData(1));
        inventory.Add(ItemDB.GetItemData(2),2);
        inventory.Add(ItemDB.GetItemData(3),2);
        HandItem = inventory[handItemIndex];

        equipamentSys.AddEquipament(ItemDB.GetItemData(5).equipamentData);


        WhenPlantUpdate += () =>{
            plantZoneData.Update();
            PlantZoneUpdater.Start();
        };
        PlantZoneUpdater = NodeMisc.GenTimer(this,1f, () =>{
            WhenPlantUpdate?.Invoke();
        });
        PlantZoneUpdater.Start();


    }


    protected override void Ready_(){
        
        HitArea = GetNode<Area2D>("HitArea");
        
        StopAttackTimer = NodeMisc.GenTimer(this, 0.5f, StopAttack);
        PlantTimer = NodeMisc.GenTimer(this,3f, StopPlanting);
        PlantCoolDownTimer = NodeMisc.GenTimer(this,0.5f, ()=> canPlant = true);

        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        CraftGUI.Deactivate();

        plantZoneData.Add("plant_test",3,0);
        PlantRange = GetNode<Area2D>("PlantRange");
        InteractableRange = GetNode<Area2D>("InteractableRange");

        PlantRange.AreaEntered += (Area2D area) => {
            if(area is PlantingZone zone){
                zone.isInRange = true;
            }
        };
        PlantRange.AreaExited += (Area2D area) => {
            if(area is PlantingZone zone){
                zone.isInRange = false;
            }
        };
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
        if(state != EntitieState.Lock) return;
        if(@event is InputEventKey KeyEvent){
            if(KeyEvent.IsActionPressed("defend")) 
                isDefending = !isDefending;
            
            if(KeyEvent.IsActionPressed("attack")) Attack();
            // else if(KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if(KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if(KeyEvent.IsActionPressed("use") && !CraftGUI.Visible) CraftGUI.Activate();
            else if(KeyEvent.IsActionPressed("use") && CraftGUI.Visible) CraftGUI.Deactivate();
            else if(KeyEvent.IsActionPressed("change_item")) ChangeItem();
        }
    }
    void ChangeItem(){
        handItemIndex++;
        if(handItemIndex >= byte.MaxValue) handItemIndex = 0;
        HandItem = inventory[(byte)(handItemIndex % inventory.Length)];
        GD.Print("handItemIndex: " + handItemIndex);
    }

    public void InteractWithSoilTile(SoilTileData data,byte quantity = 1){
        if(HandItem == null) return;
        if(ItemDB.GetItemData(HandItem.id).type == ItemType.Seed){
            Plant(data,quantity);
        }else{
            Fertilize(data,quantity);
        }
    }

    void Plant(SoilTileData data,byte quantity){
        ItemResource HandItem = UseHandItem(ItemType.Seed);
        if(HandItem == null) return;
        state = EntitieState.Lock;
        PlantTimer.Start();

    }
    void Fertilize(SoilTileData data,byte quantity){
        if(HandItem == null || !canPlant) return;
        if(HandItem.id != 7) return ;
        if(!data.AddSoilLife((byte)(quantity * 3))) return;
        GD.Print(data.soilLife);

        PlantCoolDownTimer.Start();

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

        if(handItemData.type != type) return null;
        
        if (!inventory.Remove(HandItem.id,quantity)) return null;
        if(inventory[handItemIndex] == null || HandItem.quantity == 0) HandItem = null;

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
