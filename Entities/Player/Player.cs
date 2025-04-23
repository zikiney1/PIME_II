using Godot;
using System;

public partial class Player : CharacterBody2D{
    Vector2 lastDirection;
    float Speed = (GameManager.GAMEUNITS)  * 1000;
    Area2D HitArea;
    Timer timer;
    PlayerState state = PlayerState.Idle;
    PlayerState previousState;
    Control GUI;

    EquipamentSys equipamentSys = new EquipamentSys();
    LifeSystem lifeSystem;

    enum PlayerState{
        Idle,
        Walking,
        Attacking,
        Defending,
        Dead,
        Climbing
    }

    public void Damage(Equipament[] enemyEquipaments,sbyte amount = 1) => lifeSystem.GetDamage(enemyEquipaments,amount);

    public override void _Ready()
    {
        lifeSystem = new(equipamentSys, 10);
        lifeSystem.WhenDies += Die;

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

        if(state == PlayerState.Attacking) return;
        Vector2 direction = Input.GetVector("left", "right", "up", "down");

        if(direction != Vector2.Zero){
            if(state != PlayerState.Defending)
                state = PlayerState.Walking;

            float speedTotal = ( Speed * equipamentSys.GetSpeedModifier() ) / ((state == PlayerState.Defending? 1 : 0) + 1 );
            Velocity = direction * speedTotal * (float)delta;
            lastDirection = direction;
        }else{
            state = PlayerState.Idle;
            Velocity = Vector2.Zero;
        }

		MoveAndSlide();

    }


    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey KeyEvent){
            if(KeyEvent.IsActionPressed("defend")) 
                state = state == PlayerState.Defending? PlayerState.Idle : PlayerState.Defending;
            
            if(KeyEvent.IsActionPressed("attack")) Attack();
        }
    }

    void Attack(){
        if(lastDirection == Vector2.Zero) return;

        previousState = state;
        this.state = PlayerState.Attacking;
        HitArea.Position = lastDirection * GameManager.GAMEUNITS * 3;     
        
        timer.Start();
    }
    void StopAttack(){
        this.state = PlayerState.Idle;
        HitArea.Position = Vector2.Zero;
        state = previousState;
    }


    void Die(){
        
    }

    

}
