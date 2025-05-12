using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

public class Section{
    public Dictionary<string, string> values = new();
    public string name = "";
    public string this[string key] => values[key];
    public Section(){}
    public Section(string name) => this.name = name;

    public bool Add(string key, string value){
        if(values.ContainsKey(key) && key.Trim() != "" && value.Trim() != "") return false;
        else values.Add(key, value);
        return true;
    }
    public bool IsEmpty() => values.Count == 0;
    public bool Contains(string key) => values.ContainsKey(key);
    public override string ToString() {
        string vl = $"{values.Count} values\n{values}\n";
        foreach (string item in values.Keys){
            vl +=$"{item} value: {values[item]}\n";
        }
        return vl;
    }
}

public class CfgData{
    public Dictionary<string, Section> sections = new();
    public Section this[string key] => sections[key];
    
    public bool Add(string key, Section section){
        if(sections.ContainsKey(key)|| key.Trim() == "" || section.IsEmpty()) return false;
        else sections.Add(key, section);
        return true;
    }
    public bool Add(Section section){
        return Add(section.name, section);
    }
    public bool IsEmpty() => sections.Count == 0;
    public bool Contains(string key) => sections.ContainsKey(key);
    public bool Contains(Section section) => sections.ContainsKey(section.name);

    public override string ToString()
    {
        string values = "";
        foreach (string key in sections.Keys){
            values += key + ":\n" + sections[key].ToString() + "\n";
        }
        return $"{sections.Count} sections\n{values}";
    }
}


public static class CFGParser{
    /// <summary>
    /// Parse a configuration file into a CfgData object.
    /// The configuration file should be in the format of:
    /// [section]
    /// key=value
    /// [section]
    /// key=value
    /// Each section should start with a "[" and end with a "]"
    /// and have a space between the section name and the "["
    /// and between the "]" and the next line.
    /// Each key-value pair should be separated by a "="
    /// and have a space between the key and the "="
    /// and between the "=" and the value.
    /// Any line that starts with "#" is considered a comment and is ignored.
    /// </summary>
    /// <param name="path">The path to the configuration file</param>
    /// <returns>A CfgData object that contains the configuration data</returns>
    public static CfgData Parse(string path){
        CfgData data = new();
        
        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string line;
        Section currentSection = new();
        string currentSectionName = "";

        while((line = file.GetLine()) != ""){
            if(line.StartsWith("#") || line.Trim() == "") continue;
            else if(line.StartsWith("[") && line.EndsWith("]")){
                currentSectionName = line.Substring(1, line.Length-2);
                if(!currentSection.IsEmpty()){
                    if(!data.Add(currentSection)) GD.PrintErr("was not able to add section: " + currentSectionName);
                }
                currentSection = new(currentSectionName);
            }
            else{
                int index = line.IndexOf('=');
                if(index == -1) continue;

                string key = line.Substring(0, index).Trim();
                string value;
                if (line.Length - index - 1 == 0){
                    value ="";
                }else{
                    value = line.Substring(index+1).Trim();
                }
                currentSection.Add(key, value);
            }
            
        }
        if(!data.Contains(currentSection))
            if(!data.Add(currentSection)) 
                GD.PrintErr("was not able to add section: " + currentSectionName);

        return data;
    }
    

    /// <summary>
    /// Writes a configuration data to a file.
    /// </summary>
    /// <param name="cfgData">The configuration data to write.</param>
    /// <param name="path">The path to the file to write to.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool ToFile(CfgData cfgData, string path){
        if(cfgData == null) return false;
        try{

            StringBuilder sb = new();
            cfgData.sections.Keys
            .ToList()
            .ForEach(section => {
                sb.AppendLine($"[{section}]");
                cfgData[section].values.Keys
                .ToList()
                .ForEach(key => {
                    sb.AppendLine($"{key}={cfgData[section][key]}");
                });
                sb.AppendLine();
            });

            using (FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write)){
                file.StoreString(sb.ToString());
                file.Close();
            }

        }catch(Exception e){
            GD.PrintErr(e);
            return false;
        }
        return true;
    }

}