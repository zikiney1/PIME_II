using Godot;

[GlobalClass]
public partial class RecipeResource : Resource
{
    [Export] public string name;
    [Export(PropertyHint.MultilineText)]
    public string description;
    [Export(PropertyHint.ArrayType)] public ItemResource[] ingredients;
    [Export(PropertyHint.ArrayType)] public byte[] quantity;
    [Export] public ItemResource result;
}