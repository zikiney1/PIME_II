using Godot;
using System;

public partial class ShopGui : VBoxContainer
{
    ItemList itemList;
    Button confirmButton;
    Button exitButton;
    RichTextLabel descriptionText;
    Button changeModeButton;
    public Player player;

    public ItemResource[] shopItems;
    public ItemResource[] playerInventory;

    const float sellModifier = 0.5f;
    bool inBuyMode = true;
    int selected = 2;
    int column = 0;

    public override void _Ready(){
        itemList = GetNode<ItemList>("ShopArea/HBoxContainer/ItemList");
        descriptionText = GetNode<RichTextLabel>("ShopArea/HBoxContainer/SideBar/Description");
        confirmButton = GetNode<Button>("ShopArea/HBoxContainer/SideBar/HBoxContainer/Confirm");
        exitButton = GetNode<Button>("ShopArea/HBoxContainer/SideBar/HBoxContainer/Sair");
        changeModeButton = GetNode<Button>("CharacterArea/VBoxContainer/HBoxContainer/MudaModo");

        
        player = GetNode<Player>("../..");

        itemList.ItemSelected += (index) => {
            selected = (int)index;
            itemList.Select(selected);
            OnSelectItem();
        };
        confirmButton.Pressed += WhenConfirm;
        exitButton.Pressed += Deactivate;

        changeModeButton.Pressed += () => {
            inBuyMode = !inBuyMode;
            Update();
        };
        Deactivate();
    }

    public void Activate(ItemResource[] items){
        if(items == null){
            GD.PushError("shopItems is null");
            return;
        }
        shopItems = items;
        Visible = true;
        Update();
        itemList.Select(selected);
        OnSelectItem();
    }

    public void Deactivate(){
        // Visible = false;
    }

    public void Update(){
        itemList.Clear();
        if(inBuyMode){
            confirmButton.Text = "Comprar";
            changeModeButton.Text = "Vender";
            foreach(ItemResource item in shopItems){
                int i = itemList.AddItem(item.price + "G - " + item.name,item.icon);
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


    public void OnSelectItem(){
        if(inBuyMode){
            descriptionText.Text = shopItems[selected].description;
        }else{
            descriptionText.Text = playerInventory[selected].description;
        }
    }

    public void WhenConfirm(){
        ItemResource item = shopItems[selected];
        if(inBuyMode){
            if(player.gold >= item.price){
                player.gold -= item.price;
                player.Add(item);
            }
        }else{
            player.Remove(item);
            player.gold += item.price * sellModifier;
            Update();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(!Visible || shopItems == null || shopItems.Length == 0) return;
        if(@event is InputEventKey KeyEvent){
            
            if(KeyEvent.IsActionPressed("confirm")) {
                if(column == 1) WhenConfirm();
                else if(column == 2) Deactivate();
                return;
            }else if(KeyEvent.IsActionPressed("use")) {
                Deactivate();
                return;
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

