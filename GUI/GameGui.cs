using Godot;
using System;

public partial class GameGui : VBoxContainer{
    public Player player;
    public Texture2D[] HeartzTextures;
    public Control HeartContainer;
    TextureRect[] Hearts;

    int maxHearts = 0;

    public override void _Ready(){
        player = GetNode<Player>("../..");

        HeartzTextures = [
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_complete.png"),
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_half.png"),
            GD.Load<Texture2D>(GameManager.GUIPath() + "Main/Heart_empty.png")
        ];

        HeartContainer = GetNode<Control>("MainGUI/life/HeartsHolder");

        maxHearts = player.MaxLife()/2;
        Hearts = new TextureRect[maxHearts];

        int heartSize = HeartzTextures[0].GetWidth();
        float offset = 10;
        for (int i = 0; i < maxHearts; i++){
            Hearts[i] = new();
            Hearts[i].Position = new Vector2(offset + i * (heartSize + 5),0);
            HeartContainer.AddChild(Hearts[i]);
        }
        UpdateHearts();
    }

    public void UpdateHearts(){
        for (int i = 0; i < maxHearts; i++)
            UpdateTexture(i, player.CurrentLife());
    }


    void UpdateTexture(int index, int visibleHalves){
        int halvesLeftForThisHeart = visibleHalves - index * 2;

        if(halvesLeftForThisHeart >= 2)
            Hearts[index].Texture = HeartzTextures[0];
        else if (halvesLeftForThisHeart == 1)
            Hearts[index].Texture = HeartzTextures[1];
        else
            Hearts[index].Texture = HeartzTextures[2];
    }
}
