using Godot;
[GlobalClass]
public partial class PotionEffectResource : Resource{
    [Export] public float duration = 1;
    [Export] public PotionBuilder.PotionType potionType = PotionBuilder.PotionType.Instant;
    
    [Export(PropertyHint.Range, "0,10")] public float speedAmount = 0;

    [Export(PropertyHint.Range, "0,10")] public byte healAmount = 0;
    [Export] public PotionBuilder.PotionType HealBehavior = PotionBuilder.PotionType.Instant;
    [Export(PropertyHint.Range, "0,10")] public byte damageAmount = 0;
    [Export] public PotionBuilder.PotionType DamageBehavior = PotionBuilder.PotionType.Instant;
    
    [Export] public ElementsEnum resistanceElement = ElementsEnum.Fire;
    [Export] public float resistanceAmount = 0;
    [Export] public bool AffectOtherResistance = false;

    [Export] public ElementsEnum WeakElement = ElementsEnum.Fire;
    [Export] public float weaknessAmount = 0;


}