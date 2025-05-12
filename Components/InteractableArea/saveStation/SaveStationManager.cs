using Godot;
using System;
using System.Linq;

public partial class SaveStationManager : Node
{
    [Export] public Player player;


    public void UpdateKnows(string[] saveName){
        if(saveName == null) return;
        if(saveName.Length == 0) return;
        SaveStation[] saveStation = 
                GetChildren()
                .OfType<SaveStation>()
                .Where(
                    x => 
                    saveName.Any(n => n == x.Name)
                )
                .ToArray();
                
                
        foreach(SaveStation s in saveStation){
            player.AddStation(s);
        }
        
    }
}
