using Godot;

[GlobalClass]
public partial class EquipamentDataResource : Resource
{
    [Export] public float DamageModifier;
    [Export] public float DefenseModifier;
    [Export] public float SpeedModifier;
    [Export] public ElementsEnum Element;
}