using System;
using System.Linq;
using Godot;

public class Configurator
{
    const string configPath = "res://Data/config.cfg";
    const string KEYCONFIG = "KEY_CONFIG";
    public CfgData configData;

    public static Configurator Instance;

    /// <summary>
    /// Initializes a new instance of the Configurator class.
    /// Loads configuration data from a file if it exists, otherwise creates a default configuration.
    /// Saves the configuration data to a file and sets up input keys based on the loaded configuration.
    /// </summary>
    public Configurator()
    {
        if(Instance != null) return;
        Instance = this;
        if (!FileAccess.FileExists(configPath))
        {
            DefaultConfig();
        }
        configData = CFGParser.Parse(configPath);

        SetKeys();
    }

    public void SaveConfig() => CFGParser.ToFile(configData, configPath);
    public void LoadConfig() => configData = CFGParser.Parse(configPath);

    public Section Audio => configData["AUDIO"];

    /// <summary>
    /// Creates a default configuration with the default keys.
    /// The default keys are:
    /// <list type="bullet">
    /// <item><term>LEFT</term><description>Left arrow key and D key</description></item>
    /// <item><term>RIGHT</term><description>Right arrow key and A key</description></item>
    /// <item><term>UP</term><description>Up arrow key and W key</description></item>
    /// <item><term>DOWN</term><description>Down arrow key and S key</description></item>
    /// <item><term>ATTACK</term><description>Space bar</description></item>
    /// <item><term>DEFEND</term><description>Shift key</description></item>
    /// <item><term>USE</term><description>E key</description></item>
    /// <item><term>CONFIRM</term><description>Enter key</description></item>
    /// <item><term>INVENTORY</term><description>Tab key</description></item>
    /// <item><term>CHANGE_ITEM</term><description>T key</description></item>
    /// </list>
    /// </summary>
    public void DefaultConfig()
    {
        string data = "[KEY_CONFIG]\nLEFT=A\nRIGHT=D\nUP=W\nDOWN=S\nATTACK=Space\nDEFEND=Shift\nUSE_POTION=Q\nUSE=E\nCONFIRM=Enter\nINVENTORY=Tab\nCHANGE_ITEM=T\nCHANGE_WEAPON=H\n[AUDIO]\nMASTER=0,5\nMUSICA=0,5\nSFX=0,5";
        FileAccess file = FileAccess.Open(configPath, FileAccess.ModeFlags.Write);
        file.StoreString(data);
        file.Close();
    }
    public string KeyConfig(string key)
    {
        if (configData[KEYCONFIG].Contains(key))
            return configData[KEYCONFIG][key];
        else
            return "";
    }

    public string[] GetKeys() => configData[KEYCONFIG].GetKeys().Select(x => x.ToLower()).ToArray();
    public string GetKey(string action) => configData[KEYCONFIG][action.ToUpper()];

    /// <summary>
    /// Sets up the input map based on the keys in the configuration file.
    /// Iterates over the KEYCONFIG section of the configuration file and sets up the input map for each action.
    /// </summary>
    public void SetKeys()
    {
        foreach (Section section in configData.sections.Values)
        {
            if (section.name != KEYCONFIG) continue;
            foreach (string name in section.values.Keys)
            {

                string[] keys = section.values[name].Split(',');
                SetAction(name.ToLower(), keys);
            }
        }
    }

    /// <summary>
    /// Sets up the input map for the given action name and keys.
    /// If the action already exists, it is erased and recreated.
    /// </summary>
    /// <param name="actionName">The name of the action. Must be a valid action name.</param>
    /// <param name="keys">An array of key names that will be used to trigger the action.</param>
    void SetAction(string actionName, string[] keys)
    {
        if (actionName == "" || keys.Length == 0) return;
        if (InputMap.HasAction(actionName)) InputMap.EraseAction(actionName);
        InputMap.AddAction(actionName);

        foreach (string key in keys)
        {
            if (Enum.TryParse(key, true, out Key keyCode))
            {
                InputMap.ActionAddEvent(actionName,
                    new InputEventKey()
                    {
                        Keycode = keyCode,
                    }
                );
            }
            else
            {
                GD.PushWarning($"Invalid key name: {key}");
            }
        }

    }
    void SetAction(string actionName, string key) => SetAction(actionName,[key]);


    public void SetKey(string action, string key)
    {
        if (action == "" || key == "" || action == null || key == null) return;

        if (configData[KEYCONFIG].Contains(action))
        {
            configData[KEYCONFIG].Set(action, key);
            SetAction(action, key);
        }
    }
}