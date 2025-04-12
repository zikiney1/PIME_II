using System;
using Godot;
public partial class GameManager : Node{

    public static GameManager Instance { get; set; }
    [Export] Player player;

    public CfgData configData;

    const string configPath = "res://Data/config.cfg";

    //keywords

    public readonly static byte GAMEUNITS = 32;



    public override void _Ready(){
        Instance = this;
        if(FileAccess.FileExists(configPath)){
            configData = CFGParser.Parse(configPath);
        }else{
            configData = DefaultConfig();
            CFGParser.ToFile(configData, configPath);
        }

        SetKeys();
    }

    public string KeyConfig(string key) => configData["KEY CONFIG"][key];


    public void SaveConfig() => CFGParser.ToFile(configData, configPath);
    public void LoadConfig() => configData = CFGParser.Parse(configPath);

    public CfgData DefaultConfig(){
        CfgData data = new CfgData();
        data.Add(new Section("KEY CONFIG"));
        data["KEY CONFIG"].Add("LEFT", "Left,D");
        data["KEY CONFIG"].Add("RIGHT", "Right,A");
        data["KEY CONFIG"].Add("UP", "Up,W");
        data["KEY CONFIG"].Add("DOWN", "Down,S");
        data["KEY CONFIG"].Add("ATTACK", "Space");
        
        return data;
    }


    void SetKeys(){
        
        string[] left = KeyConfig("LEFT").Split(',');
        string[] right = KeyConfig("RIGHT").Split(',');
        string[] up = KeyConfig("UP").Split(',');
        string[] down = KeyConfig("DOWN").Split(',');
        string[] attack = KeyConfig("ATTACK").Split(',');

        SetAction("left", left);
        SetAction("right", right);
        SetAction("up", up);
        SetAction("down", down);
        SetAction("attack", attack);
    }

    void SetAction(string actionName, string[] keys){
        if(InputMap.HasAction(actionName)) InputMap.EraseAction(actionName);
        else InputMap.AddAction(actionName);
        
        foreach (string key in keys)
        {
            if(Enum.TryParse(key,true, out Key keyCode)){
                InputMap.ActionAddEvent(actionName, 
                    new InputEventKey(){
                        Keycode = keyCode,
                        Pressed = true
                    }
                );
            }else{
                GD.PushWarning($"Invalid key name: {key}");
            }
        }

    }
}