using System.Linq;
using Godot;

[GlobalClass]
public partial class DialogResource : Resource
{
    [Export(PropertyHint.File)] public string DialogSequencePath;
    [Export] public string CharacterName;
    [Export] public Texture2D portrait;
    [Export] public DialogResource whenFinish;
    string[] DialogSequence;

    public string[] GetSequence(){
        if(DialogSequence != null) return DialogSequence;
        FileAccess file = FileAccess.Open(DialogSequencePath, FileAccess.ModeFlags.Read);
        DialogSequence = file.GetAsText()
                                .Replace("\r","")
                                .Split('\n')
                                .Where(x => x != "")
                                .ToArray();
        file.Close();
        return DialogSequence;
    }
}

