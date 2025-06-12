using Godot;
using System;

public partial class MenuPrincipal : Control
{
    //res://Level/Level1.tscn
    MarginContainer mainMenu;
    Configuracoes configuracoes;

    // public static MenuPrincipal Instance { get; private set; }

    public override void _Ready()
    {
        mainMenu = GetNode<MarginContainer>("MainMenu");
        configuracoes = GetNode<Configuracoes>("MarginContainer2/Configuracoes");
        configuracoes.WhenDeactivate += CloseConfig;
        mainMenu.GetNode<Button>("VBoxContainer/Novo").Pressed += () =>
        {
            GetNode<MarginContainer>("warning").Visible = true;
            mainMenu.Visible = false;
        };
        mainMenu.GetNode<Button>("VBoxContainer/Carregar").Pressed += () =>
        {
            GetTree().ChangeSceneToFile("res://Level/Level1.tscn");
        };
        mainMenu.GetNode<Button>("VBoxContainer/Carregar").Disabled = !SaveData.ContainsSaveFile();
        mainMenu.GetNode<Button>("VBoxContainer/Configuracoes").Pressed += OpenConfig;
        mainMenu.GetNode<Button>("VBoxContainer/Sair").Pressed += () =>
        {
            GetTree().Quit();
        };

        GetNode<Button>("warning/VBoxContainer/HBoxContainer/Sim").Pressed += confirmNovo;
        GetNode<Button>("warning/VBoxContainer/HBoxContainer/Nao").Pressed += () =>
        {
            GetNode<MarginContainer>("warning").Visible = false;
            mainMenu.Visible = true;
        };
        mainMenu.GetNode<Button>("VBoxContainer/Creditos").Pressed += () =>
        {
            GetTree().ChangeSceneToFile("res://GUI/TelasPrincipais/TelaCreditos.tscn");
        };
        GetNode<AnimationPlayer>("AnimationPlayer").Play("enter");
        AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        if (!audioPlayer.Playing) audioPlayer.Play();

    }

    void LoadStuff()
    {
        ItemDB.SetupItemDB();
        CraftingSystem.SetupRecipes();
        PlantingSystem.SetupPlantSystem();
        EventHandler.Setup();
        DialogManager.Setup();
    }


    public void OpenConfig()
    {
        mainMenu.Visible = false;
        configuracoes.GetParent<MarginContainer>().Visible = true;
    }
    public void CloseConfig()
    {
        mainMenu.Visible = true;
        configuracoes.GetParent<MarginContainer>().Visible = false;
    }


    void confirmNovo()
    {
        SaveData.CreateEmptySaveFile();
        GetTree().ChangeSceneToFile("res://Level/Level1.tscn");
    }
}
