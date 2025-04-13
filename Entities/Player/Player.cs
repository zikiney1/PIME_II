using Godot;
using System;

public partial class Player : CharacterBody2D{
    Vector2 lastDirection;
    float Speed = (GameManager.GAMEUNITS)  * 1000;
    sbyte lifes;
    Area2D HitArea;
    Timer timer;
    PlayerState state = PlayerState.Idle;
    PlayerState previousState;
    Control GUI;

    enum PlayerState{
        Idle,
        Walking,
        Attacking,
        Defending,
        Dead,
        Climbing
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

        if(state == PlayerState.Attacking) return;
        Vector2 direction = Input.GetVector("left", "right", "up", "down");

        if(direction != Vector2.Zero){
            if(state != PlayerState.Defending)
                state = PlayerState.Walking;

            float speedTotal = Speed / ((state == PlayerState.Defending? 1 : 0) + 1 );
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

    public void GetDamage(sbyte amount = 1){
        if(lifes - amount <= 0) {
            GameOver();
            return;
        }
        lifes -= amount;
    }

    void GameOver(){

    }

    

}
