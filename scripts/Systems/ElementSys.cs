public interface Element{
    public ElementsEnum Type();
    public ElementsEnum[] Weaknesses();
    public ElementsEnum[] Resistances();
    public float DamageModifier();
    public float DefenseModifier();
    public float SpeedModifier();
    public static Element GetElement(ElementsEnum type){
        if(type == ElementsEnum.Fire) return new FireElement();
        else if(type == ElementsEnum.Water) return new WaterElement();
        else if(type == ElementsEnum.Rock) return new RockElement();
        return null;
    }
}

public enum ElementsEnum{Fire,Water,Rock};

public class RockElement : Element{
    public ElementsEnum Type() => ElementsEnum.Rock;
    public float DamageModifier() => 0.3f;
    public float DefenseModifier() => 0.5f;
    public float SpeedModifier() => -0.3f;

    public ElementsEnum[] Weaknesses() => [];
    public ElementsEnum[] Resistances() => [ElementsEnum.Fire,ElementsEnum.Water];
}

public class FireElement : Element{
    public ElementsEnum Type() => ElementsEnum.Fire;
    public float DamageModifier() => 0.5f;
    public float DefenseModifier() => -0.3f;
    public float SpeedModifier() => 0.3f;

    public ElementsEnum[] Weaknesses() => [ElementsEnum.Rock];
    public ElementsEnum[] Resistances() => [ElementsEnum.Water];
}

public class WaterElement : Element{
    public ElementsEnum Type() => ElementsEnum.Water;
    public float DamageModifier() => -0.3f;
    public float DefenseModifier() => 0.5f;
    public float SpeedModifier() => 0.3f;

    public ElementsEnum[] Weaknesses() => [ElementsEnum.Rock];
    public ElementsEnum[] Resistances() => [ElementsEnum.Fire];
}