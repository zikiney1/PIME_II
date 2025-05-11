using Godot;
using System;

[GlobalClass]
public partial class PlantResource : Resource
{
    [Export] public Texture2D[] GrowthProcess;
    [Export] public Texture2D DeadPlant;
    [Export] public short growthDurationSeconds;

    [Export] public ItemResource result;
    [Export] public byte resultQuantity;

    public ItemResource seed;
    public string name;
}
