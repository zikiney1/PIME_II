using Godot;
[GlobalClass]
public partial class PotionEffectResource : Resource{
    [Export] public float duration = 1;
    [Export] public bool useLevel = true;
    
    [Export(PropertyHint.Range, "0,10")] public float speedAmount = 0;

    [Export(PropertyHint.Range, "0,10")] public byte healAmount = 0;
    [Export] public PotionBuilder.PotionType HealBehavior = PotionBuilder.PotionType.Instant;
    [Export(PropertyHint.Range, "0,10")] public byte takeDamageAmount = 0;
    [Export] public PotionBuilder.PotionType TakeDamageBehavior = PotionBuilder.PotionType.Instant;

    [Export(PropertyHint.Range, "0,10")] public byte damageAmount = 0;
    [Export] public float defenseAmount = 0;



}