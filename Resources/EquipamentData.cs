using Godot;

[GlobalClass]
public partial class EquipamentData : Resource
{
    [Export] public float DamageModifier;
    [Export] public float DefenseModifier;
    [Export] public float SpeedModifier;
    [Export] public float AttackSpeedModifier;

}