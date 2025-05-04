using Godot;
using System;

public partial class GameGui : VBoxContainer{
    public Player player;
    public Texture2D[] HeartzTextures;
    public HBoxContainer HeartContainer;
    HeartIcon[] Hearts;

    int maxHearts = 0;

    public override void _Ready(){
        player = GetNode<Player>("../..");

        HeartzTextures = [
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_complete.png"),
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_half.png"),
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_empty.png")
        ];

        HeartContainer = GetNode<HBoxContainer>("MainGUI/life/HeartsHolder");

        maxHearts = player.MaxLife()/2;
        Hearts = new HeartIcon[maxHearts];

        int heartSize = HeartzTextures[0].GetWidth();
        for (int i = 0; i < maxHearts; i++){
            Hearts[i] = new(heartSize,HeartzTextures);
            HeartContainer.AddChild(Hearts[i]);
        }
        UpdateHearts();
    }

    public void UpdateHearts(){
        for (int i = 0; i < maxHearts; i++)
            Hearts[i].Update(player.CurrentLife(), i);
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