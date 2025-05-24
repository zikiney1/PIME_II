using Godot;
using System;

public partial class GameGui : VBoxContainer{
    public Player player;
    public Texture2D[] HeartzTextures;
    public HBoxContainer HeartContainer;
    public RichTextLabel HandItemName;
    public RichTextLabel HandItemQuantity;
    public RichTextLabel CoinAmount;
    public TextureRect HandItemIcon;
    public ItemList inventory;
    HeartIcon[] Hearts;

    int maxHearts = 0;

    Timer animationTimer;
    AnimationPlayer animationPlayer;

    Action whenAnimationEnds;

    public override void _EnterTree()
    {
        HeartzTextures = [
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_complete.png"),
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_half.png"),
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_empty.png")
        ];

        HeartContainer = GetNode<HBoxContainer>("MainGUI/life/HeartsHolder");

        HandItemName = GetNode<RichTextLabel>("MainGUI/ItemRegion/ItemName");
        HandItemIcon = GetNode<TextureRect>("MainGUI/ItemRegion/ItemLayout/Portrait/ItemIcon");
        HandItemQuantity = GetNode<RichTextLabel>("MainGUI/ItemRegion/ItemLayout/Portrait/QuantityLabel");
        CoinAmount = GetNode<RichTextLabel>("MainGUI/VBoxContainer/HBoxContainer/CoinAmount");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        inventory = GetNode<ItemList>("GameContainter/HBoxContainer/ItemList");

        animationTimer = NodeMisc.GenTimer(this, 0.5f, () =>
        {
            whenAnimationEnds?.Invoke();
            animationPlayer.Stop(true);
        });
        inventory.Clear();
        inventory.Visible = false;
    }

    /// <summary>
    /// Initializes the game GUI by setting up the player node, loading heart textures,
    /// and configuring the heart container and hand item display.
    /// </summary>
    /// <remarks>
    /// This method retrieves the player node, loads textures for heart icons, and initializes
    /// the heart container with HeartIcon instances based on the player's maximum life.
    /// Additionally, it sets up the GUI elements for displaying the player's current hand item,
    /// including the item name, icon, and quantity.
    /// </remarks>
    public void Setup()
    {

        player = Player.Instance;

        maxHearts = player.MaxLife() / 2;
        Hearts = new HeartIcon[maxHearts];

        int heartSize = HeartzTextures[0].GetWidth();
        for (int i = 0; i < maxHearts; i++)
        {
            Hearts[i] = new(heartSize, HeartzTextures);
            HeartContainer.AddChild(Hearts[i]);
        }
        UpdateHearts();
        UpdateGold();
        InventoryUpdate();
    }

    public void InventoryUpdate(){
        inventory.Clear();
        if(player == null) return;
        if (player.inventory == null) return;
        if(player.inventory.items == null) return;


        foreach (ItemData item in player.inventory.items)
        {
            inventory.AddItem(item.resource.name + " - " + item.quantity, item.resource.icon);
        }
    }

    public void OpenInventory()
    {
        inventory.Visible = true;
        animationPlayer.Play("open");
        animationTimer.WaitTime = animationPlayer.CurrentAnimationLength;

    }
    public void CloseInventory()
    {
        animationPlayer.Play("close");
        animationTimer.WaitTime = animationPlayer.CurrentAnimationLength;
        whenAnimationEnds = () => { inventory.Visible = false; };
        animationTimer.Start();
    }

    public void UpdateHearts()
    {
        for (int i = 0; i < maxHearts; i++)
            Hearts[i].Update(player.CurrentLife(), i);
    }

    public void UpdateHandItem(string name,Texture2D icon, int quantity){
        HandItemIcon.Texture = icon;
        HandItemName.Text = name;
        
        if(quantity >= 1)
            HandItemQuantity.Text = quantity.ToString();
        else
            HandItemQuantity.Text = "";
    }
    public void SetEmptyHandItem(){
        HandItemIcon.Texture = null;
        HandItemName.Text = "";
        HandItemQuantity.Text = "";
    }


    public void UpdateGold()
    {
        CoinAmount.Text = " " + player.gold.ToString();
    }
}




public partial class HeartIcon : TextureRect{
    Texture2D[] HeartzTextures;
    public HeartIcon(float minSize,Texture2D[] HeartzTextures){
        CustomMinimumSize = new Vector2(minSize, minSize);
        StretchMode = StretchModeEnum.Keep;
        this.HeartzTextures = HeartzTextures;
    }
    public void Update(int life,int index){
        int halvesLeftForThisHeart = life - index * 2;

        if(halvesLeftForThisHeart >= 2)
            Texture = HeartzTextures[0];
        else if (halvesLeftForThisHeart == 1)
            Texture = HeartzTextures[1];
        else
            Texture = HeartzTextures[2];
    }
}