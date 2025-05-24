using Godot;
using System;

public partial class QuestData : Resource
{
    [Export] public byte id;
    [Export] public string Name;
    [Export] public string Description;
    [Export(PropertyHint.ArrayType)] public ItemResource[] reward;
    [Export] public int goldReward;
    public Action onCompleted;
    public bool completed=false;
    public bool active = false;

    public void Complete()
    {
        active = false;
        completed = true;
        onCompleted?.Invoke();
    }
}
