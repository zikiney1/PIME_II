using System;
using Godot;

public class Configurator{
    const string configPath = "res://Data/config.cfg";
    const string KEYCONFIG = "KEY_CONFIG";
    public CfgData configData;

    public Configurator(){
       if(FileAccess.FileExists(configPath)){
            configData = CFGParser.Parse(configPath);
        }else{
            configData = DefaultConfig();
            CFGParser.ToFile(configData, configPath);
        }

        SetKeys();
    }

    public void SaveConfig() => CFGParser.ToFile(configData, configPath);
    public void LoadConfig() => configData = CFGParser.Parse(configPath);



    public CfgData DefaultConfig(){
        CfgData data = new CfgData();
        data.Add(new Section(KEYCONFIG));
        data[KEYCONFIG].Add("LEFT", "Left,D");
        data[KEYCONFIG].Add("RIGHT", "Right,A");
        data[KEYCONFIG].Add("UP", "Up,W");
        data[KEYCONFIG].Add("DOWN", "Down,S");
        data[KEYCONFIG].Add("ATTACK", "Space");
        data[KEYCONFIG].Add("DEFEND", "Shift");
        
        return data;
    }
    public string KeyConfig(string key){
        if(configData[KEYCONFIG].Contains(key))
            return configData[KEYCONFIG][key];
        else
            return "";
    }

    void SetKeys(){
        
        string[] left = KeyConfig("LEFT").Split(',');
        string[] right = KeyConfig("RIGHT").Split(',');
        string[] up = KeyConfig("UP").Split(',');
        string[] down = KeyConfig("DOWN").Split(',');
        string[] attack = KeyConfig("ATTACK").Split(',');
        string[] defend = KeyConfig("DEFEND").Split(',');

        SetAction("left", left);
        SetAction("right", right);
        SetAction("up", up);
        SetAction("down", down);
        SetAction("attack", attack);
        SetAction("defend", defend);
    }

    void SetAction(string actionName, string[] keys){
        if(actionName == "" || keys.Length == 0) return;
        if(InputMap.HasAction(actionName)) InputMap.EraseAction(actionName);
        InputMap.AddAction(actionName);
        
        foreach (string key in keys)
        {
            if(Enum.TryParse(key,true, out Key keyCode)){
                InputMap.ActionAddEvent(actionName, 
                    new InputEventKey(){
                        Keycode = keyCode,
                    }
                );
            }else{
                GD.PushWarning($"Invalid key name: {key}");
            }
        }

    }
}