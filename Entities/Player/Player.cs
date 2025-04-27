using Godot;
using System;

public partial class Player : Entitie{
    Area2D HitArea;
    Timer timer;
    
    Control GUI;

    bool isDefending = false;




    public override void _EnterTree()
    {
        Speed = (GameManager.GAMEUNITS)  * 1000;
        entitieModifier = new();
        equipamentSys = new(entitieModifier);
        inventory = new(9);
        lifeSystem = new(entitieModifier, 5,10);
        lifeSystem.WhenDies += Die;
    }


    public override void _Ready()
    {
        lastDirection = new();
        HitArea = GetNode<Area2D>("HitArea");
        timer = NodeFac.GenTimer(this, 0.5f, StopAttack);
        previousState = state;
        GUI = GetNode<Control>("Canvas/GameGUI");
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
        if(lastDirection == Vector2.Zero) return;

        previousState = state;
        this.state = EntitieState.Attacking;
        HitArea.Position = lastDirection * GameManager.GAMEUNITS * 3;     
        
        timer.Start();
    }
    protected override void StopAttack(){
        this.state = EntitieState.Idle;
        HitArea.Position = Vector2.Zero;
        state = previousState;
    }


    protected override void Die(){
        
    }

    protected override void UsePotion(){
        if(HandItem == null) return;
        if(HandItem.type != ItemType.Potion) return;

        HandItem.effect.Apply(this);
    }

}
