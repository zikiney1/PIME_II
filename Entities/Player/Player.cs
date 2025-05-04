using Godot;
using System;

public partial class Player : Entitie{
    Area2D HitArea;
    Timer StopAttackTimer;
    Timer PlantZoneUpdater;
    
    public Action WhenPlantUpdate;

    GameGui GUI;
    CraftingGui CraftGUI;

    bool isDefending = false;

    public void UpdateHearts() => GUI.UpdateHearts();

    public InventorySystem inventory;
    public EquipamentSys equipamentSys;
    public PlantZoneData plantZoneData = new (3,3,[
        100,70,100,
        100,20,100,
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

        inventory.Add(ItemDB.GetItemData(1));
        inventory.Add(ItemDB.GetItemData(2),2);
        inventory.Add(ItemDB.GetItemData(3),2);
        HandItem = inventory[0];

        equipamentSys.AddEquipament(ItemDB.GetItemData(5).equipamentData);


        WhenPlantUpdate+= () =>{
            plantZoneData.Update();
            PlantZoneUpdater.Start();
        };
        PlantZoneUpdater = NodeMisc.GenTimer(this,1f, () =>{
            WhenPlantUpdate?.Invoke();
        });
        PlantZoneUpdater.Start();
    }


    protected override void Ready_()
    {
        HitArea = GetNode<Area2D>("HitArea");
        StopAttackTimer = NodeMisc.GenTimer(this, 0.5f, StopAttack);
        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        CraftGUI.Deactivate();

        plantZoneData.Add("plant_test",3,0);
    }

    public override void _PhysicsProcess(double delta){

    }

    public override void _Process(double delta)
    {
        if(state == EntitieState.Attacking || CraftGUI.Visible) return;
        Vector2 direction = InputSystem.GetVector();

        float speedTotal = ( Speed * entitieModifier.GetSpeedModifier() ) / (MathM.BoolToInt(Input.IsActionPressed("defend")) + 1 );
        Walk(direction, delta,speedTotal);

    }


    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey KeyEvent){
            if(KeyEvent.IsActionPressed("defend")) 
                isDefending = !isDefending;
            
            if(KeyEvent.IsActionPressed("attack")) Attack();
            else if(KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if(KeyEvent.IsActionPressed("use") && !CraftGUI.Visible) CraftGUI.Activate();
            else if(KeyEvent.IsActionPressed("use") && CraftGUI.Visible) CraftGUI.Deactivate();
        }
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

    void UsePotion(){
        if(HandItem == null) return;
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);
        if(handItemData == null || handItemData.effect == null) return;

        if(handItemData.type != ItemType.Potion) return;

        handItemData.effect.Apply(this);
        inventory.Remove(HandItem.id);
        if(HandItem.quantity == 0) HandItem = null;
    }
}
