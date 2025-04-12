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
    public static CfgData Parse(string path){
        CfgData data = new();
        
        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string[] lines = file.GetAsText().Split("\n");

        Section currentSection = new();
        string currentSectionName = "";

        foreach(string line in lines){
            if(line.StartsWith("#") || line.Trim() == "") continue;
            else if(line.StartsWith("[") && line.EndsWith("]")){
                string sectionName = line.Substring(1, line.Length-2);
                bool result = data.Add(currentSection);
                if(!result) GD.PrintErr("was not able to add section: " + currentSectionName);
                currentSection = new(sectionName);
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
        
        if(!data.Add(currentSection)) GD.PrintErr("was not able to add section: " + currentSectionName);
        return data;
    }
    

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