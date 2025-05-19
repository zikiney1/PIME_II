using Godot;
using System;
using System.Linq;

public partial class SaveStationManager : Node
{
    public Player player;


    public override void _EnterTree()
    {
        player = Player.Instance;
    }

    /// <summary>
    /// Updates the know save stations of the player.
    /// <para>
    /// This function takes an array of save station names and updates the player's known save stations.
    /// </para>
    /// <para>
    /// If the given array is null or empty, the function does nothing.
    /// </para>
    /// </summary>
    public void UpdateKnows(string[] saveName)
    {
        if (saveName == null) return;
        if (saveName.Length == 0) return;
        SaveStation[] saveStation =
                GetChildren()
                .OfType<SaveStation>()
                .Where(
                    x =>
                    saveName.Any(n => n == x.Name)
                )
                .ToArray();


        foreach (SaveStation s in saveStation)
        {
            player.AddStation(s);
        }

    }
}
