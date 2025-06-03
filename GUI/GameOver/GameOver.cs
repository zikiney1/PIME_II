using Godot;
using System;

public partial class GameOver : PanelContainer
{
    AnimationPlayer animationPlayer;
    Player player => Player.Instance;
    
    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        Visible = false;
        GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/reload").Pressed += RestartScene;
        GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/sair").Pressed += GoToMainMenu;

    }
    public void Activate()
    {
        Visible = true;
        animationPlayer.Play("enter");
    }

    void RestartScene()
    {
        GetTree().ReloadCurrentScene();
    }
    void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
}
