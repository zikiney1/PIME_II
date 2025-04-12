using Godot;
using System;

public partial class Player : CharacterBody2D{
    Vector2 lastDirection;
    float Speed = (GameManager.GAMEUNITS/2)  * 1000;
    sbyte lifes;
    Area2D HitArea;
    Timer timer;
    PlayerState state = PlayerState.Idle;

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
    }

    public override void _PhysicsProcess(double delta){

    }

    public override void _Process(double delta)
    {

        if(state == PlayerState.Attacking) return;
        Vector2 direction = Input.GetVector("left", "right", "up", "down");

        if(direction != Vector2.Zero){
            state = PlayerState.Walking;
            Velocity = direction * Speed * (float)delta;
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
            if(KeyEvent.IsActionPressed("attack")) Attack();   
        }
    }

    void Attack(){
        if(lastDirection == Vector2.Zero) return;
        this.state = PlayerState.Attacking;
        HitArea.Position = lastDirection * GameManager.GAMEUNITS * 3;     
        
        timer.Start();
    }
    void StopAttack(){
        this.state = PlayerState.Idle;
        HitArea.Position = Vector2.Zero;
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
