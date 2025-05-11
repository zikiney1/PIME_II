using Godot;

[GlobalClass]
public partial class EquipamentData : Resource
{
    [Export] public float DamageModifier;
    [Export] public float DefenseModifier;
    [Export] public float SpeedModifier;
    [Export] ElementsEnum elementType;
    Element element;
    public ItemResource item;
    public byte GetId() => item.id;

    public EquipamentData SetElement(){
        element = Element.GetElement(elementType);
        return this;
    }

    public ElementsEnum Type() => elementType;
    public ElementsEnum[] Weaknesses() => element.Weaknesses();
    public ElementsEnum[] Resistances() => element.Resistances();
}