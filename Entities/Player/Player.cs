using Godot;
using System;

public partial class Player : Entitie{
    Area2D HitArea;
    Timer timer;
    
    GameGui GUI;
    CraftingGui CraftGUI;

    bool isDefending = false;

    public void UpdateHearts() => GUI.UpdateHearts();

    


    public override void _EnterTree()
    {
        Speed = (GameManager.GAMEUNITS)  * 500;
        entitieModifier = new();
        equipamentSys = new(entitieModifier);
        inventory = new(9);
        lifeSystem = new(entitieModifier, 5,10);
        lifeSystem.WhenDies += Die;

        inventory.Add(new Item(1));
        HandItem = inventory[0];

        equipamentSys.AddEquipament(ItemDB.GetItemData(5).equipamentData);
    }


    protected override void Ready_()
    {
        HitArea = GetNode<Area2D>("HitArea");
        timer = NodeFac.GenTimer(this, 0.5f, StopAttack);
        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        CraftGUI.Deactivate();
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
        GetTree().ReloadCurrentScene();
    }



    public override void WhenTakeDamage(){
        UpdateHearts();
    }

    public override void whenHeal(){
        UpdateHearts();
    }

}
