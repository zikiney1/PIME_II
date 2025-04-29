using Godot;


[GlobalClass]
public partial class RecipeResource : Resource{
    [Export]
    public string name;
    [Export(PropertyHint.MultilineText)]
    public string description;
    [Export(PropertyHint.ArrayType, "ItemResource")]
    public ItemResource[] ingredients;
    [Export]
    public ItemResource result;
}