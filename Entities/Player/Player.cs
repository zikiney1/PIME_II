using Godot;
using System;


public enum PlayerState{
    Idle,
    Walking,
    Attacking,
    Dead,
    Lock
}
public partial class Player : CharacterBody2D
{
    public enum PlayerWeapon { Sword, Zarabatana }


    [Export] public PlantingZone PlantZone;
    [Export] public SaveStationManager checkPointManager;

    Area2D HitArea;
    Area2D InteractableRange;
    GameManager gameManager;


    Timer PlantZoneUpdater;
    Timer PlantTimer;
    Timer PlantCoolDownTimer;

    public Action WhenPlantUpdate;

    GameGui GUI;
    CraftingGui CraftGUI;
    ShopGui ShopGUI;
    SaveStationGui saveGUI;
    DialogGui dialogGui;

    bool isDefending = false;
    bool canPlant = true;
    public byte handItemIndex = 0;
    public float PlantRange = GameManager.GAMEUNITS * 1.5f;
    public float gold = 1;
    float Speed = (GameManager.GAMEUNITS) * 1000;
    Vector2 lastDirection;
    Vector2 MouseDirection;

    ItemData HandItem = null;
    public PotionModifier potionModifier = new();
    public PlayerWeapon playerWeapon = PlayerWeapon.Sword;

    PlayerState state = PlayerState.Idle;
    PlayerState previousState = PlayerState.Idle;


    public InventorySystem inventory;
    public EquipamentSys equipamentSys;
    public PlantZoneData plantZoneData;
    public LifeSystem lifeSystem;
    public FightSystem fightSystem;

    public void Save(Vector2 pos) => SaveData.Save(this, pos);
    public void UpdateHearts() => GUI.UpdateHearts();
    public void AddStation(SaveStation saveStation) => saveGUI.AddStation(saveStation);
    public void UpdateKnowsCheckPoints(string[] names) => checkPointManager.UpdateKnows(names);
    public string[] KnowCheckPoints() => saveGUI.ToNames();

    public void Heal(int amount = 1) => lifeSystem.Heal(amount);
    public int CurrentLife() => lifeSystem.CurrentLife();
    public int MaxLife() => lifeSystem.MaxLife();

    public Texture2D BulletTexture = GD.Load<Texture2D>("res://assets/Sprites/test/player_projectile.png");
    public float bulletSpeed = 300f;

    public override void _EnterTree()
    {
        inventory = new();
        equipamentSys = new();
        fightSystem = new(this, 0.4f, 1f);

        fightSystem.WhenStopAttack += StopAttack;

        PlantTimer = NodeMisc.GenTimer(this, 3f, StopPlanting);
        PlantCoolDownTimer = NodeMisc.GenTimer(this, 0.5f, () => canPlant = true);

        HitArea = GetNode<Area2D>("HitArea");
        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        ShopGUI = GetNode<ShopGui>("Canvas/ShopGUI");
        saveGUI = GetNode<SaveStationGui>("Canvas/SaveStationGUI");
        InteractableRange = GetNode<Area2D>("InteractableRange");
        dialogGui = GetNode<DialogGui>("Canvas/DialogGUI");
        gameManager = GetNode<GameManager>("..");
    }


    public override void _Ready()
    {
        Speed = GameManager.GAMEUNITS * 350;

        SaveData.LoadSaveFile(this);
        plantZoneData.WhenUpdate += PlantZone.Update;
        GUI.Setup();

        lifeSystem.WhenDies += Die;

        WhenPlantUpdate += () =>
        {
            plantZoneData.Update();
            PlantZoneUpdater.Start();
        };
        PlantZoneUpdater = NodeMisc.GenTimer(this, 1f, () =>
        {
            WhenPlantUpdate?.Invoke();
        });
        PlantZoneUpdater.Start();

        HandItem = inventory[handItemIndex];

        if (HandItem == null)
        {
            ChangeHandItem();
        }
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);

        GUI.UpdateHandItem(handItemData.name, handItemData.icon, HandItem.quantity);

        HitArea.BodyEntered += whenHitEnemy;
    }

    public override void _PhysicsProcess(double delta)
    {

    }

    public override void _Process(double delta)
    {
        if (state == PlayerState.Attacking || CraftGUI.Visible || state == PlayerState.Lock) return;

        Vector2 direction = InputSystem.GetVector();

        float speedModifier = equipamentSys.speed + potionModifier.speed + 1;
        float speedTotal = (Speed * speedModifier) / (MathM.BoolToInt(Input.IsActionPressed("defend")) + 1);

        if (direction != Vector2.Zero)
        {
            state = PlayerState.Walking;
            Velocity = direction * speedTotal * (float)delta;
            lastDirection = direction;
        }
        else
        {
            state = PlayerState.Idle;
            Velocity = Vector2.Zero;
        }

        MoveAndSlide();

    }


    public override void _Input(InputEvent @event)
    {
        if (state == PlayerState.Lock) return;
        if (@event is InputEventKey KeyEvent)
        {
            if (KeyEvent.IsActionPressed("defend"))
                isDefending = !isDefending;

            // if (KeyEvent.IsActionPressed("attack") && playerWeapon == PlayerWeapon.Sword) Attack();
            if (KeyEvent.IsActionPressed("attack")) Attack();
            else if (KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if (KeyEvent.IsActionPressed("change_item")) ChangeHandItem();
            else if (Input.IsKeyPressed(Key.H)) switchWeapon();


        }
        if (@event is InputEventMouseMotion mouseMove)
        {
            MouseDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
        }
        if(@event is InputEventMouseButton mouseButton){
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && playerWeapon == PlayerWeapon.Zarabatana){
                Attack();
            }
        }
    }

    void switchWeapon()
    {
        if (playerWeapon == PlayerWeapon.Sword) playerWeapon = PlayerWeapon.Zarabatana;
        else playerWeapon = PlayerWeapon.Sword;
        HitArea.GetNode<CollisionShape2D>("CollisionShape2D").SetDisabled(playerWeapon != PlayerWeapon.Sword);
    }

    public void InteractDialog(DialogResource[] dialog, string EventAtEnd)
    {
        if (dialogGui.Visible){
            dialogGui.Deactivate();
            state = PlayerState.Idle;
        }
        else{
            dialogGui.Activate(dialog, EventAtEnd);
            state = PlayerState.Lock;
        }
    }

    public void InteractMerchant(ItemResource[] shopItems)
    {
        if (ShopGUI.Visible){
            ShopGUI.Deactivate();
            state = PlayerState.Idle;
        }
        else{
            ShopGUI.Activate(shopItems);
            state = PlayerState.Lock;
        }
    }

    public void InteractCraft()
    {
        if (CraftGUI.Visible){
            CraftGUI.Deactivate();
            state = PlayerState.Idle;
        }
        else{
            CraftGUI.Activate();
            state = PlayerState.Lock;
        }

    }

    public void InteractSaveStation(SaveStation saveStation)
    {
        if (saveGUI.Visible)
        {
            saveGUI.Deactivate();
            state = PlayerState.Idle;
        }
        else
        {
            saveGUI.Activate(saveStation);
            state = PlayerState.Lock;
        }
    }

    /// <summary>
    /// Adds the specified quantity of the given item to the inventory.
    /// If the item being added is the same as the current hand item, updates the item portrait.
    /// </summary>
    /// <param name="item">The item to add to the inventory.</param>
    /// <param name="quantity">The quantity of the item to add. Defaults to 1.</param>
    /// <returns>True if the item was added to an existing stack, otherwise false.</returns>
    public bool Add(ItemResource item, byte quantity = 1)
    {
        bool result = inventory.Add(item, quantity);
        if (HandItem != null)
            if (HandItem.id == item.id)
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
    public bool Remove(ItemResource item, byte quantity = 1)
    {
        bool result = inventory.Remove(item.id, quantity);
        if (HandItem.id == item.id)
            UpdatePortrait();
        return result;
    }

    /// <summary>
    /// Updates the portrait of the hand item in the GUI.
    /// </summary>
    void UpdatePortrait()
    {
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);
        GUI.UpdateHandItem(handItemData.name, handItemData.icon, HandItem.quantity);
    }


    /// <summary>
    /// Changes the hand item to the next item in the inventory, if none are available, sets the hand item to null.
    /// </summary>
    void ChangeHandItem()
    {
        handItemIndex++;
        if (handItemIndex >= inventory.Length) handItemIndex = 0;
        HandItem = inventory[handItemIndex];

        if (HandItem == null)
        {
            ChangeHandItem();
        }
        else
        {
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
    public void InteractWithSoilTile(SoilTileData data, byte quantity = 1)
    {
        if (HandItem == null) return;
        if (ItemDB.GetItemData(HandItem.id).type == ItemType.Seed)
        {
            Plant(data);
        }
        else
        {
            Fertilize(data, quantity);
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
    void Plant(SoilTileData data)
    {
        ItemResource HandItemResource = UseHandItem(ItemType.Seed);
        if (HandItemResource == null || data.ContainsPlant() || !canPlant) return;
        if (HandItemResource.plantData == null) return;

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
    void Fertilize(SoilTileData data, byte quantity)
    {
        if (HandItem == null || !canPlant) return;
        if (HandItem.id != 7) return;

        if (!data.AddSoilLife((byte)(quantity * 3))) return;
        UseHandItem(ItemType.Resource, quantity);
    }

    /// <summary>
    /// Stops planting. Sets the player state to idle.
    /// </summary>
    public void StopPlanting()
    {
        state = PlayerState.Idle;
    }
    /// <summary>
    /// Teleports the player to the specified position and sets the state to idle.
    /// </summary>
    /// <param name="pos">The target position to teleport the player to.</param>
    public void Teleport(Vector2 pos)
    {
        state = PlayerState.Idle;
        Position = pos;
    }

    /// <summary>
    /// Uses the current hand item as a potion.
    /// If the hand item is a potion, its effect is applied to the player.
    /// </summary>
    void UsePotion()
    {
        ItemResource handItemData = UseHandItem(ItemType.Potion);
        if (handItemData == null || handItemData.effect == null) return;

        handItemData.effect.Apply(this);
    }

    /// <summary>
    /// Uses the current hand item if it matches the given type and there is enough of it in the inventory.
    /// If the hand item is used, it is removed from the inventory and the player's hand item is updated.
    /// </summary>
    /// <param name="type">The type of item to use.</param>
    /// <param name="quantity">The quantity of the item to use. Defaults to 1.</param>
    /// <returns>The item data of the used item, or null if the item was not used.</returns>
    ItemResource UseHandItem(ItemType type, byte quantity = 1)
    {
        if (HandItem == null || quantity <= 0) return null;
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);

        if (handItemData == null) return null;
        if (handItemData.type != type || !inventory.Contains(HandItem.id, quantity)) return null;

        if (!inventory.Remove(HandItem.id, quantity)) return null;
        if (inventory[handItemIndex] == null || HandItem.quantity == 0) HandItem = null;

        if (HandItem != null)
        {
            GUI.UpdateHandItem(handItemData.name, handItemData.icon, HandItem.quantity);
        }
        else
        {
            GUI.UpdateHandItem("", null, 0);
        }

        return handItemData;
    }


    public void AddGold(int amount)
    {
        gold += amount;
        GUI.UpdateGold();
    }

    public bool RemoveGold(int amount)
    {
        if (gold - amount < 0) return false;
        gold -= amount;
        GUI.UpdateGold();
        return true;
    }
    public bool CanPurchase(int amount)
    {
        return gold - amount >= 0;
    }

    void Attack()
    {
        if (!fightSystem.canAttack) return;
        float attackSpeedmodifier = -(equipamentSys.attackSpeed + potionModifier.attackSpeed);

        if (playerWeapon == PlayerWeapon.Sword)
        {
            HitArea.Position = lastDirection * GameManager.GAMEUNITS;
            HitArea.Rotation = lastDirection.Angle();
        }
        else
        {
            var e = gameManager.GetBullet(this, GlobalPosition, MouseDirection);
            e.SetTexture(BulletTexture);
            e.WhenBodyEnter = whenHitEnemy;
            e.speed = bulletSpeed;
        }

        if (state == PlayerState.Attacking) return;
        previousState = state;
        this.state = PlayerState.Attacking;
        fightSystem.Attack(attackSpeedmodifier);
    }

    void StopAttack()
    {
        state = previousState;
        HitArea.Position = Vector2.Zero;
        HitArea.Rotation = 0;
    }
    void Die()
    {
        GD.Print("YOU DIED");
    }
    void whenHeal()
    {
        UpdateHearts();
    }

    public void Damage(int amount)
    {
        if (fightSystem.isInvencible) return;
        float modifier = potionModifier.defense + equipamentSys.defense;
        lifeSystem.GetDamage(modifier, amount);
        UpdateHearts();
        fightSystem.GetDamage();
    }


    public void whenHitEnemy(Node2D body)
    {
        if (body.IsInGroup("EnemyGroup"))
        {
            float damageModifier = -(equipamentSys.damage + potionModifier.damage);
            if (playerWeapon == PlayerWeapon.Zarabatana) damageModifier /= 2;
            if (body is Atirador a) a.Damage(damageModifier);
        }
    }

}
