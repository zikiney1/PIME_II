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
    public static Player Instance;
    public enum PlayerWeapon { Sword, Zarabatana }

    [Export] public PlantingZone PlantZone;
    [Export] public SaveStationManager checkPointManager;

    Area2D HitArea;
    Area2D InteractableRange;
    StaticBody2D ShieldCollision;
    GameManager gameManager;


    Timer PlantZoneUpdater;
    Timer PlantTimer;
    Timer PlantCoolDownTimer;
    Timer AnimationTimer;

    public PlayerAudioManager audioManager;

    public Action WhenPlantUpdate;

    GameGui GUI;
    CraftingGui CraftGUI;
    ShopGui ShopGUI;
    SaveStationGui saveGUI;
    DialogGui dialogGui;
    MainMenu mainMenu;

    public bool isDefending = false;
    bool canPlant = true;
    public byte handItemIndex = 0;
    public float PlantRange = GameManager.GAMEUNITS;
    public int gold = 0;
    float Speed = GameManager.GAMEUNITS * 200;
    public bool canAttack = false;
    Vector2 lastDirection;
    Vector2 MouseDirection;
    public Vector2 direction;

    ItemData HandItem = null;
    public PotionModifier potionModifier = new();
    public PlayerWeapon playerWeapon = PlayerWeapon.Sword;

    PlayerState state = PlayerState.Idle;
    PlayerState previousState = PlayerState.Idle;

    Action whenAnimationEnds;

    public InventorySystem inventory;
    public EquipamentSys equipamentSys;
    public PlantZoneData plantZoneData;
    public LifeSystem lifeSystem;
    public FightSystem fightSystem;
    public AnimationHandler animationHandler;
    public ReflectionHandler reflectionHandler;
    public ColorRect blur;
    public GameOver gameOver;

    public void Save() => SaveData.Save(this);
    public void UpdateHearts() => GUI.UpdateHearts();
    public void AddStation(SaveStation saveStation) => saveGUI.AddStation(saveStation);
    public string[] KnowCheckPoints() => saveGUI.ToNames();
    public bool CanPurchase(int amount) => gold - amount >= 0;

    public int CurrentLife() => lifeSystem.CurrentLife();
    public int MaxLife() => lifeSystem.MaxLife();

    public Texture2D BulletTexture = GD.Load<Texture2D>("res://assets/Sprites/test/player_projectile.png");
    public float bulletSpeed = 300f;

    public override void _EnterTree()
    {
        Instance = this;
        inventory = new();
        equipamentSys = new();
        fightSystem = new(this, 0.4f, 1.7f);

        fightSystem.WhenStopAttack += StopAttack;

        PlantTimer = NodeMisc.GenTimer(this, 3f, StopPlanting);
        PlantCoolDownTimer = NodeMisc.GenTimer(this, 0.5f, () => canPlant = true);

        HitArea = GetNode<Area2D>("HitArea");
        ShieldCollision = GetNode<StaticBody2D>("shield");
        GUI = GetNode<GameGui>("Canvas/GameGUI");
        CraftGUI = GetNode<CraftingGui>("Canvas/CraftingGUI");
        ShopGUI = GetNode<ShopGui>("Canvas/ShopGUI");
        saveGUI = GetNode<SaveStationGui>("Canvas/SaveStationGUI");
        InteractableRange = GetNode<Area2D>("InteractableRange");
        dialogGui = GetNode<DialogGui>("Canvas/DialogGUI");
        mainMenu = GetNode<MainMenu>("Canvas/MainMenu");
        gameOver = GetNode<GameOver>("Canvas/GameOver");
        blur = GetNode<ColorRect>("Canvas/blur");
        animationHandler = new(
            GetNode<AnimationPlayer>("Animations/CharacterAnimationPlayer"),
            GetNode<AnimationPlayer>("Animations/HitAnimationPlayer")
        );
        reflectionHandler = new(
            GetNode<Sprite2D>("Sprite"),
            GetNode<Sprite2D>("WaterReflection")
        );
        audioManager = GetNode<PlayerAudioManager>("AudioManager");

        AnimationTimer = NodeMisc.GenTimer(this, 0.1f, () => { whenAnimationEnds?.Invoke(); });

        gameManager = GameManager.Instance;

        HitArea.GetNode<CollisionShape2D>("CollisionShape2D").SetDisabled(true);
        blur.Visible = false;

    }


    public override void _Ready()
    {
        audioManager.PlaySong(PlayerAudioManager.SongToPlay.Overworld);
        SaveData.TryLoadSaveFile(this);
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

        if (HandItem != null)
        {
            ItemResource handItemData = ItemDB.GetItemData(HandItem.id);
            GUI.UpdateHandItem(handItemData.name, handItemData.icon, HandItem.quantity);
        }
        else
        {
            GUI.SetEmptyHandItem();
        }


        HitArea.BodyEntered += whenHitEnemy;

        animationHandler.Play("idle");

        if (GlobalPosition == new Vector2(64, 128))
            EventHandler.EmitEvent("OnStart");

    }

    public override void _PhysicsProcess(double delta)
    {
        if (state == PlayerState.Attacking || CraftGUI.Visible || state == PlayerState.Lock || state == PlayerState.Dead) return;

        direction = InputSystem.GetVector();

        float speedModifier = equipamentSys.speed + potionModifier.speed + 1;
        isDefending = Input.IsActionPressed("defend");

        float speedTotal;
        if (isDefending && canAttack)
        {
            ShieldCollision.Position = direction * (GameManager.GAMEUNITS / 4);
            ShieldCollision.Rotation = direction.Angle();
            ShieldCollision.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
            speedTotal = (Speed * speedModifier) / (MathM.BoolToInt(isDefending) + 1);
            animationHandler.Defend(direction);
        }
        else
        {
            ShieldCollision.GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
            speedTotal = (Speed * speedModifier);
            animationHandler.Direction(direction);
        }

        if (direction != Vector2.Zero)
        {
            state = PlayerState.Walking;
            Velocity = direction * speedTotal * (float)delta;
            lastDirection = direction;
            audioManager.PlayWalk();
        }
        else
        {
            state = PlayerState.Idle;
            Velocity = Vector2.Zero;
            audioManager.StopSFX();
        }

        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        if (state == PlayerState.Dead) return;
        reflectionHandler.Update();
    }


    public override void _Input(InputEvent @event)
    {
        if (state == PlayerState.Lock || state == PlayerState.Dead) return;
        if (@event is InputEventKey KeyEvent)
        {
            if (KeyEvent.IsActionPressed("defend"))
                isDefending = !isDefending;

            // if (KeyEvent.IsActionPressed("attack") && playerWeapon == PlayerWeapon.Sword) Attack();
            if (KeyEvent.IsActionPressed("attack")) Attack();
            else if (KeyEvent.IsActionPressed("use_potion")) UsePotion();
            else if (KeyEvent.IsActionPressed("change_item")) ChangeHandItem();
            else if (KeyEvent.IsActionPressed("change_weapon")) switchWeapon();
            else if (KeyEvent.IsActionPressed("inventory")) ToggleInventory();
            else if (KeyEvent.IsActionPressed("ui_cancel")) mainMenu.Open();


        }
        if (@event is InputEventMouseMotion mouseMove)
        {
            MouseDirection = (GetGlobalMousePosition() - GlobalPosition).Normalized();
        }
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed && playerWeapon == PlayerWeapon.Zarabatana)
            {
                Attack();
            }
        }
    }

    //============================================================================================================
    //============================================================================================================

    public void DeactivateBlur() => blur.Visible = false;
    public void ActivateBlur() => blur.Visible = true;
    public void SetDialog(string dialogRaw, string EventAtEnd) => dialogGui.SetDialog(dialogRaw, EventAtEnd);
    public void InteractDialog(string dialogPath, string EventAtEnd)
    {
        if (dialogGui.isPlayingAnimation) return;
        if (dialogGui.Visible || dialogPath == "")
        {
            dialogGui.Deactivate();
            state = PlayerState.Idle;
            blur.Visible = false;
        }
        else
        {
            dialogGui.Activate(dialogPath, EventAtEnd);
            state = PlayerState.Lock;
            blur.Visible = true;
        }
    }

    public void InteractMerchant(ItemResource[] shopItems, Texture2D merchantBanner = null)
    {
        if (ShopGUI.Visible)
        {
            ShopGUI.Deactivate();
            state = PlayerState.Idle;
            blur.Visible = false;
        }
        else
        {
            ShopGUI.Activate(shopItems, merchantBanner);
            state = PlayerState.Lock;
            blur.Visible = true;
        }
    }

    public void InteractCraft()
    {
        if (CraftGUI.Visible)
        {
            CraftGUI.Deactivate();
            state = PlayerState.Idle;
            blur.Visible = false;
        }
        else
        {
            CraftGUI.Activate();
            state = PlayerState.Lock;
            blur.Visible = true;
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
        if(data.ContainsPlant() || !canPlant) return;
        ItemResource HandItemResource = UseHandItem(ItemType.Seed);
        if (HandItemResource == null) return;
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

    //============================================================================================================
    //============================================================================================================

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

        GUI.InventoryUpdate();

        return result;
    }
    public bool Add(byte id, byte quantity = 1) => Add(ItemDB.GetItemData(id), quantity);
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

        GUI.InventoryUpdate();
        return result;
    }
    public bool Remove(byte id, byte quantity = 1) => Remove(ItemDB.GetItemData(id), quantity);

    public void Heal(int amount = 1)
    {
        lifeSystem.Heal(amount);
        UpdateHearts();
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

    //============================================================================================================
    //============================================================================================================


    public void SetHandItem(int index)
    {
        if (index < 0) return;
        if (inventory.IsHandItem(index))
        {
            HandItem = inventory[(byte)index];
            GUI.UpdateHandItem(HandItem.name, HandItem.icon, HandItem.quantity);
        }
    }

    public void SetEquipament(byte id)
    {
        ItemResource item = ItemDB.GetItemData(id);
        if (item == null) return;
        if (inventory.Contains(id) == false) return;

        inventory.Remove(id);
        Add(equipamentSys.equipament);
        equipamentSys.RemoveEquipament();
        equipamentSys.AddEquipament(item);

        GUI.SetEquipament(item.name, item.icon);
        GUI.InventoryUpdate();
    }

    /// <summary>
    /// Updates the portrait of the hand item in the GUI.
    /// </summary>
    void UpdatePortrait()
    {
        ItemResource handItemData = ItemDB.GetItemData(HandItem.id);
        GUI.UpdateHandItem(handItemData.name, handItemData.icon, HandItem.quantity);
    }

    void ToggleInventory()
    {

        if (GUI.inventory.Visible)
        {
            GUI.CloseInventory();
        }
        else
        {
            GUI.OpenInventory();
        }
    }

    //============================================================================================================
    //============================================================================================================

    /// <summary>
    /// Changes the hand item to the next item in the inventory, if none are available, sets the hand item to null.
    /// </summary>
    void ChangeHandItem()
    {
        handItemIndex++;
        if (inventory.Length == 0 || handItemIndex >= inventory.Length )
        {
            GUI.SetEmptyHandItem();
            return;
        }
        if (inventory.IsHandItem(handItemIndex))
        {
            HandItem = inventory[handItemIndex];
            UpdatePortrait();
        }
        else
        {
            ChangeHandItem();
        }

    }
    void switchWeapon()
    {
        if (playerWeapon == PlayerWeapon.Sword) playerWeapon = PlayerWeapon.Zarabatana;
        else playerWeapon = PlayerWeapon.Sword;
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
        GUI.InventoryUpdate();
        return handItemData;
    }

    //============================================================================================================
    //============================================================================================================


    /// <summary>
    /// Teleports the player to the specified position and sets the state to idle.
    /// </summary>
    /// <param name="pos">The target position to teleport the player to.</param>
    public void Teleport(Vector2 pos)
    {
        state = PlayerState.Lock;
        whenAnimationEnds = () =>
        {
            whenAnimationEnds = () =>
            {
                state = PlayerState.Idle;
            };
            Position = pos;
            Camera.Instance.UpdateScreen(pos);
            animationHandler.Play("teleport_end");
            AnimationTimer.WaitTime = animationHandler.GetAnimationTime();
            AnimationTimer.Start();
        };
        animationHandler.Play("teleport_start");
        AnimationTimer.WaitTime = animationHandler.GetAnimationTime();
        AnimationTimer.Start();
    }

    public void UpdateKnowsCheckPoints(string[] names)
    {
        if (checkPointManager.player == null) checkPointManager.player = this;
        checkPointManager.UpdateKnows(names);
    }
    //============================================================================================================
    //============================================================================================================

    void Attack()
    {
        if (!fightSystem.canAttack || !canAttack) return;
        float attackSpeedmodifier = -(equipamentSys.attackSpeed + potionModifier.attackSpeed);

        if (playerWeapon == PlayerWeapon.Sword)
        {
            HitArea.Position = lastDirection * (GameManager.GAMEUNITS / 2);
            HitArea.Rotation = lastDirection.Angle();
            HitArea.GetNode<CollisionShape2D>("CollisionShape2D").SetDisabled(false);
            audioManager.PlayAttack();
            animationHandler.Attack(lastDirection);
        }
        else
        {
            var e = gameManager.GetBullet(HitArea.CollisionMask, GlobalPosition, MouseDirection);
            e.SetTexture(BulletTexture);
            e.WhenBodyEnter = whenHitEnemy;
            e.speed = bulletSpeed;
            audioManager.PlayZarabatan();
            if(MouseDirection.X < 0)
                animationHandler.Play("attack2_left");
            else if(MouseDirection.X > 0)
                animationHandler.Play("attack2_right");
            else if(MouseDirection.Y < 0)
                animationHandler.Play("attack2_up");
            else if(MouseDirection.Y > 0)
                animationHandler.Play("attack2_down");
            // animationHandler.Attack2(MouseDirection);
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
        HitArea.GetNode<CollisionShape2D>("CollisionShape2D").SetDisabled(true);
    }

    void Die()
    {
        state = PlayerState.Dead;
        gameOver.Activate();
        animationHandler.Die();
    }

    public void Damage(int amount)
    {
        if (fightSystem.isInvencible || state == PlayerState.Dead) return;
        float modifier = potionModifier.defense + equipamentSys.defense;
        lifeSystem.GetDamage(modifier, amount);
        UpdateHearts();
        fightSystem.GetDamage();
        animationHandler.Damage();
    }


    public void whenHitEnemy(Node2D body)
    {
        if (body.IsInGroup("EnemyGroup"))
        {
            float damageModifier = -(equipamentSys.damage + potionModifier.damage);
            if (playerWeapon == PlayerWeapon.Zarabatana) damageModifier /= 2;
            if (body is Atirador a) a.Damage(damageModifier);
            else if (body is Rolante r) r.Damage(damageModifier);
            else if (body is EspadachinPlanta e) e.Damage(damageModifier);
            else if (body is CachorroPlanta c) c.Damage(damageModifier);
            else if (body is Boss b) b.Damage(damageModifier);
        }
    }

    public void WinState()
    {
        state = PlayerState.Lock;
        animationHandler.Play("win_fx");        
    }
}
