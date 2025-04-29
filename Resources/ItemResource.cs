using Godot;

[GlobalClass]
public partial class ItemResource : Resource{
    [Export] public byte id = 1;
    [Export] public byte level = 1;
    [Export] public string name = "";
    [Export(PropertyHint.MultilineText)] public string description = "";
    [Export] public Texture2D iconFile = null;
    [Export(PropertyHint.Range, "0,500")] public float price = 0;
    [Export(PropertyHint.Range, "1,25")] public byte stackMaxSize = 1;
    [Export] public ItemType type = ItemType.Ingredient;
    [Export] public PotionEffectResource PotionEffect = null;
    [Export] public EquipamentData equipamentData = null;
    public PotionEffect effect;

}