using Godot;
using System;

public class PotionEffect{
    public Action<Player> whenApply;
    public Action whenStop;
    public Action whenUpdate;
    public Player player;

    public virtual void Apply(Player player){
        this.player = player;
        whenApply?.Invoke(player);
    }
}

public class PeriodicEffect : PotionEffect{
    public PeriodicEffect(float duration) => this.duration = duration;
    protected float duration = 3;
    protected bool isToStop = false;
    Timer durationTimer, UpdateTimer;

    public override void Apply(Player player) {
        base.Apply(player);

        durationTimer = new();
        durationTimer.WaitTime = duration;
        durationTimer.OneShot = true;
        durationTimer.Timeout += whenStop;

        UpdateTimer = new();
        UpdateTimer.WaitTime = 1f;
        UpdateTimer.Timeout += whenUpdate;

        whenUpdate += () => {
            if(isToStop) whenStop?.Invoke();
        };

        whenStop += () => {
            isToStop = true;
            durationTimer.Stop();
            UpdateTimer.Stop();
        };

        player.AddChild(durationTimer);
        player.AddChild(UpdateTimer);
    }
}


public class TimedEffect : PotionEffect{
    protected float duration = 3;
    public TimedEffect(float duration) => this.duration = duration;
    public override void Apply(Player player) {
        base.Apply(player);
        Timer timer = NodeFac.GenTimer(player, duration, whenStop);
        player.AddChild(timer);
    }
}

public class InstaEffect : PotionEffect{
    public override void Apply(Player player){
        base.Apply(player);
    }
}

public class PotionBuilder{
    public enum PotionType{
        Instant,
        Periodic,
        Timed
    }

    public PotionEffect effect;
    public PotionBuilder(PotionType type, float duration = 2f){
        if(type == PotionType.Instant) effect = new InstaEffect();
        else if(type == PotionType.Periodic) effect = new PeriodicEffect(duration);
        else if(type == PotionType.Timed) effect = new TimedEffect(duration);
    }

    public PotionBuilder SetDamageOverTime(int amount = 1){
        effect.whenUpdate += () => {
            effect.player.Damage(amount);
        };
        return this;
    }
    public PotionBuilder SetHealOverTime(int amount = 1){
        effect.whenUpdate += () => {
            effect.player.Heal(amount);
        };
        return this;
    }
    public PotionBuilder SetHealInstant(int amount = 1){
        effect.whenApply += (Player player) => {
            player.Heal(amount);
        };
        return this;
    }
    public PotionBuilder SetResistenceThreshold(Element element, float amount){
        foreach (ElementsEnum elementType in element.Weaknesses()){
            SetResistence(elementType,-amount);
        }
        foreach (ElementsEnum elementType in element.Resistances()){
            SetResistence(elementType,amount);
        }
        return this;
    }
    public PotionBuilder SetDamage(ElementsEnum element, float amount){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionDamageModifier((Half)amount,element);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionDamageModifier(element);
        return this;
    }
    public PotionBuilder SetResistence(ElementsEnum element, float amount){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionResistenceModifier((Half)amount,element);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionResistenceModifier(element);
        return this;
    }
    public PotionBuilder SetSpeed(float amount){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionSpeedModifier((Half)amount);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionSpeedModifier();
        return this;
    }

    public PotionEffect Build() => effect;

}


public class PotionFactory{
    public static int AmountWithLevel(int amount,int level){
        return amount + (int)Math.Round((float)(amount/5) * (level-1));
    }
    public static float AmountWithLevel(float amount,int level){
        return (float)(amount + Math.Round((amount/5) * (level-1)));
    }

    public static PotionBuilder HealingOverTime(int amount,float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Periodic,duration);
        builder.SetHealOverTime(AmountWithLevel(amount,level));
        return builder;
    }
    public static PotionBuilder HealInstant(int amount,int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Instant);
        builder.SetHealInstant(AmountWithLevel(amount,level));
        return builder;
    }

    public static PotionBuilder HealAndDamageOverTime(int healAmount, int damageAmount, float duration, int level){
        PotionBuilder builder = HealInstant(healAmount,level);
        builder.SetHealOverTime(AmountWithLevel(damageAmount,level));
        return builder;
    }

    public static PotionBuilder DamageAndHealOverTime(int damageAmount, int healAmount, float duration, int level){
        PotionBuilder builder = HealingOverTime(healAmount,duration,level);
        builder.SetDamageOverTime(AmountWithLevel(damageAmount,level));
        return builder;
    }

    public static PotionBuilder SpeedPotion(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.SetSpeed(AmountWithLevel(amount,level));
        return builder;
    }

    public static PotionBuilder FireResistance(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.SetResistence(ElementsEnum.Fire,AmountWithLevel(amount,level));
        return builder;
    }
    public static PotionBuilder WaterResistance(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.SetResistence(ElementsEnum.Water,AmountWithLevel(amount,level));
        return builder;
    }
    public static PotionBuilder RockResistance(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.SetResistence(ElementsEnum.Rock,AmountWithLevel(amount,level));
        return builder;
    }

    public static PotionBuilder DamageAndResistOverTime(int damageAmount, ElementsEnum element, float resistenceAmount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Periodic,duration);

        if(element == ElementsEnum.Fire) builder.SetResistence(ElementsEnum.Fire,AmountWithLevel(resistenceAmount,level));
        else if(element == ElementsEnum.Water) builder.SetResistence(ElementsEnum.Water,AmountWithLevel(resistenceAmount,level));
        else builder.SetResistence(ElementsEnum.Rock,AmountWithLevel(resistenceAmount,level));;
        
        builder.SetDamageOverTime(AmountWithLevel(damageAmount,level));
        return builder;
    }
}