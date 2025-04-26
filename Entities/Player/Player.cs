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

    EquipamentSys equipamentSys = new();
    LifeSystem lifeSystem;
    InventorySystem inventory;

    bool isDefending = false;

    byte[] Hands = new byte[2];

    enum PlayerState{
        Idle,
        Walking,
        Attacking,
        Dead,
        Climbing
    }

    public void Damage(Equipament[] enemyEquipaments,sbyte amount = 1) => lifeSystem.GetDamage(enemyEquipaments,amount);
    public void Damage(Element element,sbyte amount = 1) => lifeSystem.GetDamage(element,amount);
    public void Damage(sbyte amount = 1) => lifeSystem.GetDamage(1f,amount);
    public int MaxLife() => lifeSystem.MaxLife();
    public int CurrentLife() => lifeSystem.CurrentLife();
    public void Heal(byte amount = 1) => lifeSystem.Heal(amount);


    public override void _EnterTree()
    {
        inventory = new(9);
        lifeSystem = new(equipamentSys, 5,10);
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

        if(state == PlayerState.Attacking) return;
        Vector2 direction = Input.GetVector("left", "right", "up", "down");

        if(direction != Vector2.Zero){
            state = PlayerState.Walking;
            // if(state != PlayerState.Defending)

            float speedTotal = ( Speed * equipamentSys.GetSpeedModifier() ) / (MathM.BoolToInt(isDefending) + 1 );
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
                isDefending = !isDefending;
            
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

    void UsePotion(int hand){
        ItemData potionItem = inventory[Hands[hand]];
        if(potionItem == null) return;
        if(potionItem.type != ItemType.Potion) return;

        potionItem.effect.Apply(this);
    }

}
