using Godot;
using System;

public partial class ShopGui : VBoxContainer
{
    ItemList itemList;
    Button confirmButton;
    Button exitButton;
    RichTextLabel descriptionText;
    Button changeModeButton;
    RichTextLabel AlertText;
    public TextureRect MerchantTexture;

    public ItemResource[] shopItems;
    public ItemResource[] playerInventory;


    const float sellModifier = 0.5f;
    bool inBuyMode = true;
    int selected = 2;
    int column = 0;

    public Player player;
    Timer AlertAnimation;
    Timer AlertEnd;
    int currentLetter=0;

    public override void _Ready(){
        itemList = GetNode<ItemList>("ShopArea/HBoxContainer/MarginContainer2/ItemList");
        descriptionText = GetNode<RichTextLabel>("ShopArea/HBoxContainer/MarginContainer/SideBar/Description");
        confirmButton = GetNode<Button>("ShopArea/HBoxContainer/MarginContainer/SideBar/HBoxContainer/Confirm");
        exitButton = GetNode<Button>("ShopArea/HBoxContainer/MarginContainer/SideBar/HBoxContainer/Sair");
        changeModeButton = GetNode<Button>("CharacterArea/VBoxContainer/HBoxContainer/MudaModo");
        AlertText = GetNode<RichTextLabel>("CharacterArea/VBoxContainer/HBoxContainer/Alert(no mone)");
        MerchantTexture = GetNode<TextureRect>("CharacterArea/MerchantImage");

        AlertText.Visible = false;
        AlertAnimation = NodeMisc.GenTimer(this, 0.03f, AlertAnimationTick);
        AlertEnd = NodeMisc.GenTimer(this, 2f, ()=>{
            AlertText.Visible=false;
            currentLetter = 0;
        });
        
        player = GetNode<Player>("../..");

        itemList.ItemSelected += (index) => {
            selected = (int)index;
            itemList.Select(selected);
            OnSelectItem();
        };
        confirmButton.Pressed += WhenConfirm;
        exitButton.Pressed += () =>
        {
            player.InteractMerchant(null);
            Deactivate();
        };

        changeModeButton.Pressed += () => {
            inBuyMode = !inBuyMode;
            Update();
        };
        Deactivate();
    }

    /// <summary>
    /// Activates the shop GUI, making it visible and setting the shop items.
    /// </summary>
    /// <param name="items">The items to display in the shop.</param>
    /// <remarks>
    /// <para>
    /// This function makes the shop GUI visible and sets the items in the shop item list.
    /// </para>
    /// <para>
    /// If the items parameter is null, it does nothing and logs an error.
    /// </para>
    /// <para>
    /// It also selects the currently selected item in the item list.
    /// </para>
    /// </remarks>
    public void Activate(ItemResource[] items, Texture2D MerchantBanner = null){
        if(items == null){
            GD.PushError("shopItems is null");
            return;
        }
        MerchantTexture.Texture = MerchantBanner;
        shopItems = items;
        Visible = true;
        Update();
        itemList.Select(selected);
        OnSelectItem();
    }

    public void Deactivate()
    {
        Visible = false;
        MerchantTexture.Texture = null;
    }

    
    /// <summary>
    /// Updates the shop GUI item list and button texts based on the current mode (buy or sell).
    /// In buy mode, it populates the item list with shop items, displaying their price and name.
    /// In sell mode, it populates the item list with player inventory items, displaying their sell price and name.
    /// </summary>
    public void Update(){
        itemList.Clear();
        if(inBuyMode){
            confirmButton.Text = "Comprar";
            changeModeButton.Text = "Vender";
            foreach(ItemResource item in shopItems){
                itemList.AddItem(item.price + "G - " + item.name,item.icon);
            }
        }else{
            confirmButton.Text = "Vender";
            changeModeButton.Text = "Comprar";
            playerInventory = new ItemResource[player.inventory.items.Count];

            for(byte i = 0; i < playerInventory.Length; i++){
                playerInventory[i] = player.inventory[i].resource;
            }
            foreach(ItemResource item in playerInventory){
                itemList.AddItem((item.price * sellModifier) + " - " + item.name,item.icon);
            }
        }
    }


    /// <summary>
    /// Updates the description text with the description of the selected item.
    /// If in buy mode, it uses the description of the selected shop item.
    /// If not in buy mode, it uses the description of the selected player item.
    /// </summary>
    public void OnSelectItem(){
        if(inBuyMode){
            descriptionText.Text = shopItems[selected].description;
        }else{
            descriptionText.Text = playerInventory[selected].description;
        }
    }

    /// <summary>
    /// Handles when the confirm button is pressed.
    /// If in buy mode, if the player has enough gold, the item is bought and added to the player's inventory.
    /// If not in buy mode, the item is removed from the player's inventory and the gold is added to the player.
    /// </summary>
    public void WhenConfirm(){
        ItemResource item = shopItems[selected];
        if(inBuyMode){
            if(player.CanPurchase(item.price)){
                player.RemoveGold(item.price);
                player.Add(item);
            }else{
                AlertText.Visible = true;
                AlertAnimation.Start();
                AlertEnd.Stop();
                currentLetter = 0;
                AlertText.VisibleCharacters = currentLetter;
            }
        }else{
            player.Remove(item);
            player.AddGold((int)Math.Round(item.price * sellModifier,0));
            Update();
        }
    }

    void AlertAnimationTick(){
        if(currentLetter >= AlertText.GetTotalCharacterCount()){
            AlertEnd.Start();
            return;
        }
        currentLetter++;
        AlertText.VisibleCharacters = currentLetter;
        AlertAnimation.Start();
    }

    /// <summary>
    /// Handles user input for the shop GUI.
    /// </summary>
    /// <param name="event">The input event to handle.</param>
    /// <remarks>
    /// This function handles key presses for the shop GUI.
    /// <list type="bullet">
    /// <item>
    /// <term>Up and Down arrows</term>
    /// <description>Select the previous or next item in the shop.</description>
    /// </item>
    /// <item>
    /// <term>Left and Right arrows</term>
    /// <description>Navigate between the item list, confirm button, and exit button.</description>
    /// </item>
    /// <item>
    /// <term>Confirm button</term>
    /// <description>Buy or sell the selected item.</description>
    /// </item>
    /// <item>
    /// <term>Exit button</term>
    /// <description>Exit the shop.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public override void _Input(InputEvent @event)
    {
        if(!Visible || shopItems == null || shopItems.Length == 0) return;
        if(@event is InputEventKey KeyEvent){
            
            if(KeyEvent.IsActionPressed("confirm")) {
                if(column == 2) player.InteractMerchant(null);
                else WhenConfirm();
                // return;
            }else if(KeyEvent.IsActionPressed("inventory")) {
                inBuyMode = !inBuyMode;
                Update();
            }


            itemList.Deselect(selected);
            itemList.ReleaseFocus();
            exitButton.ReleaseFocus();
            confirmButton.ReleaseFocus();

            Vector2 inputDir = InputSystem.GetVector();
            int YDirection = (int)inputDir.Y;
            int XDirection = (int)inputDir.X;

            if((YDirection == 1 || YDirection == -1 ) && column == 0){

                if(YDirection + selected > shopItems.Length-1) selected = 0;
                else if(YDirection + selected < 0) selected = shopItems.Length-1;
                else selected += YDirection;
                
                itemList.Select(selected);

                OnSelectItem();

            }else{
                column += XDirection;
                if(column <= -1) column = 2;
                else if(column >= 3) column = 0;

                if(column == 1){
                    confirmButton.GrabFocus();
                }else if(column == 2){
                    exitButton.GrabFocus();
                }else{
                    column = 0;
                    itemList.Select(selected);
                }
            }

        }
    }

}

