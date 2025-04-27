using Godot;
using System;

public partial class Player : Entitie{
    Area2D HitArea;
    Timer timer;
    
    GameGui GUI;

    bool isDefending = false;

    public void UpdateHearts() => GUI.UpdateHearts();


    public override void _EnterTree()
    {
        Speed = (GameManager.GAMEUNITS)  * 1000;
        entitieModifier = new();
        equipamentSys = new(entitieModifier);
        inventory = new(9);
        lifeSystem = new(entitieModifier, 5,10);
        lifeSystem.WhenDies += Die;

        inventory.Add(new Item(1));
        HandItem = inventory[0];
    }


    protected override void Ready_()
    {
        HitArea = GetNode<Area2D>("HitArea");
        timer = NodeFac.GenTimer(this, 0.5f, StopAttack);
        GUI = GetNode<GameGui>("Canvas/GameGUI");
    }

    public override void _PhysicsProcess(double delta){

    }

    public override void _Process(double delta)
    {

        if(state == EntitieState.Attacking) return;
        Vector2 direction = Input.GetVector("left", "right", "up", "down");

        float speedTotal = ( Speed * entitieModifier.GetSpeedModifier() ) / (MathM.BoolToInt(isDefending) + 1 );
        Walk(direction, delta,speedTotal);

    }


    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey KeyEvent){
            if(KeyEvent.IsActionPressed("defend")) 
                isDefending = !isDefending;
            
            if(KeyEvent.IsActionPressed("attack")) Attack();
            if(KeyEvent.IsActionPressed("use")) UsePotion();
        }
    }

    protected override void Attack(){
        HitArea.Position = lastDirection * GameManager.GAMEUNITS * 3;  
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
