using Godot;

[GlobalClass]
public partial class RecipeResource : Resource
{
    [Export(PropertyHint.ArrayType)] public ItemResource[] ingredients;
    [Export(PropertyHint.ArrayType)] public byte[] quantity;
    [Export] public ItemResource result;
}