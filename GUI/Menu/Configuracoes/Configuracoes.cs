using Godot;
using System;

public partial class Configuracoes : HBoxContainer
{
    PackedScene actionScene = GD.Load<PackedScene>("res://GUI/Menu/Configuracoes/KeyBind/action.tscn");
    VBoxContainer keyList;
    bool isRemaping = false;
    string actionToRemap = "";
    string oldName = "";
    Button remapingButton;
    Configurator config => manager.configurator;
    GameManager manager;

    PanelContainer KeyBind;
    PanelContainer Audio;

    int MasterBusIndex,SFXBusIndex,MusicBusIndex;
    public Action WhenDeactivate;

    public override void _Ready()
    {
        manager = GameManager.Instance;
        KeyBind = GetNode<PanelContainer>("PanelContainer/KeyBind");
        Audio = GetNode<PanelContainer>("PanelContainer/Audio");

        MasterBusIndex = AudioServer.GetBusIndex("Master");
        SFXBusIndex = AudioServer.GetBusIndex("SFX");
        MusicBusIndex = AudioServer.GetBusIndex("Musica");

        Audio.Visible = false;
        KeyBind.Visible = true;

        GetNode<Button>("Opções/Lista/Teclas").Pressed += () =>
        {
            Audio.Visible = false;
            KeyBind.Visible = true;
        };
        GetNode<Button>("Opções/Lista/Audio").Pressed += () =>
        {
            Audio.Visible = true;
            KeyBind.Visible = false;
        };

        HSlider master = GetNode<HSlider>("PanelContainer/Audio/MarginContainer/VBoxContainer/HBoxContainer/HSlider");
        HSlider music = GetNode<HSlider>("PanelContainer/Audio/MarginContainer/VBoxContainer/HBoxContainer2/HSlider");
        HSlider sfx = GetNode<HSlider>("PanelContainer/Audio/MarginContainer/VBoxContainer/HBoxContainer3/HSlider");

        master.Value = float.Parse(config.Audio["MASTER"]);
        music.Value = float.Parse(config.Audio["MUSICA"]);
        sfx.Value = float.Parse(config.Audio["SFX"]);

        master.ValueChanged += WhenMasterAudioChange;
        music.ValueChanged += WhenMusicAudioChange;
        sfx.ValueChanged += WhenSFXAudioChange;

        keyList = KeyBind.GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ScrollContainer/ActionList");
        
        CreateList();
    }

    public void CreateList()
    {
        foreach (var item in keyList.GetChildren())
            item.QueueFree();

        foreach (string act in config.GetKeys())
        {
            string key = config.GetKey(act);
            AddAction(act, key);

        }
    }

    public void AddAction(string name, string key)
    {
        Button button = actionScene.Instantiate<Button>();
        button.GetNode<Label>("MarginContainer/HBoxContainer/ActionName").Text = name;
        button.GetNode<Label>("MarginContainer/HBoxContainer/ActionInput").Text = key.Replace("(Physical)", "");

        keyList.AddChild(button);
        button.Pressed += () => WhenActionPressed(button, key);

    }

    public override void _Input(InputEvent @event)
    {
        if(!Visible) return;
        if (@event is InputEventKey KeyEvent)
        {
            if (isRemaping)
            {
                if (KeyEvent.Keycode == Key.Escape)
                {
                    remapingButton.GetNode<Label>("MarginContainer/HBoxContainer/ActionInput").Text = oldName;
                }
                else
                {
                    string key = KeyEvent.Keycode.ToString();
                    config.SetKey(actionToRemap, key);
                    remapingButton.GetNode<Label>("MarginContainer/HBoxContainer/ActionInput").Text = key;

                }
                isRemaping = false;
            }
            if (KeyEvent.Keycode == Key.Escape)
            {
                Exit();
            }

        }

    }
    void Exit()
    {
        Visible = false;
        WhenDeactivate?.Invoke();
        config.SaveConfig();
    }

    void WhenMasterAudioChange(double value)
    {
        AudioServer.SetBusVolumeDb(MasterBusIndex, Mathf.LinearToDb((float)value));
        config.Audio.Set("MASTER", value.ToString());
    }
    void WhenSFXAudioChange(double value)
    {
        AudioServer.SetBusVolumeDb(SFXBusIndex, Mathf.LinearToDb((float)value));
        config.Audio.Set("SFX", value.ToString());
    }
    void WhenMusicAudioChange(double value)
    {
        AudioServer.SetBusVolumeDb(MusicBusIndex, Mathf.LinearToDb((float)value));
        config.Audio.Set("MUSICA", value.ToString());
    }
    
    
    void WhenActionPressed(Button button, string name)
    {
        if (!isRemaping)
        {
            isRemaping = true;
            actionToRemap = name;
            remapingButton = button;
            oldName = button.GetNode<Label>("MarginContainer/HBoxContainer/ActionInput").Text;
            button.GetNode<Label>("MarginContainer/HBoxContainer/ActionInput").Text = "Precione alguma tecla...";
        }
    }
}
