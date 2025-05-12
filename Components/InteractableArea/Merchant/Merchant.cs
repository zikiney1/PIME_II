using Godot;
using System;

public partial class Merchant : Interactable
{
    [Export(PropertyHint.ArrayType)] ItemResource[] shopItems;
    public override void Interact(){
        base.Interact();
        if(player == null) return;
        if(shopItems == null){
            GD.PushError("shopItems is null");
            return;
        }
        player.InteractMerchant(shopItems);
    }

}
