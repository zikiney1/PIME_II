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

    public PotionBuilder TakeDamageOverTime(int amount = 1){
        effect.whenUpdate += () => {
            effect.player.Damage(amount);
        };
        return this;
    }
    public PotionBuilder TakeDamageInstant(int amount = 1){
        effect.whenApply += (Player player) => {
            player.Damage(amount);
        };
        return this;
    }
    public PotionBuilder HealOverTime(int amount = 1){
        effect.whenUpdate += () => {
            effect.player.Heal(amount);
        };
        return this;
    }
    public PotionBuilder HealInstant(int amount = 1){
        effect.whenApply += (Player player) => {
            player.Heal(amount);
        };
        return this;
    }
    
    public PotionBuilder Damage(ElementsEnum element, float amount){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionDamageModifier((Half)amount,element);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionDamageModifier(element);
        return this;
    }
    public PotionBuilder Resistence(ElementsEnum element, float amount){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionResistenceModifier((Half)amount,element);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionResistenceModifier(element);
        return this;
    }
    public PotionBuilder Resistence(Element element, float amount){
        foreach (ElementsEnum elementType in element.Weaknesses()){
            Resistence(elementType,-amount);
        }
        foreach (ElementsEnum elementType in element.Resistances()){
            Resistence(elementType,amount);
        }
        return this;
    }
    public PotionBuilder Speed(float amount){
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
        builder.HealOverTime(AmountWithLevel(amount,level));
        return builder;
    }
    public static PotionBuilder HealInstant(int amount,int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Instant);
        builder.HealInstant(AmountWithLevel(amount,level));
        return builder;
    }

    public static PotionBuilder HealAndDamageOverTime(int healAmount, int damageAmount, float duration, int level){
        PotionBuilder builder = HealInstant(healAmount,level);
        builder.HealOverTime(AmountWithLevel(damageAmount,level));
        return builder;
    }

    public static PotionBuilder TakeDamageAndHealOverTime(int damageAmount, int healAmount, float duration, int level){
        PotionBuilder builder = HealingOverTime(healAmount,duration,level);
        builder.TakeDamageOverTime(AmountWithLevel(damageAmount,level));
        return builder;
    }

    public static PotionBuilder SpeedPotion(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.Speed(AmountWithLevel(amount,level));
        return builder;
    }

    public static PotionBuilder FireResistance(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.Resistence(ElementsEnum.Fire,AmountWithLevel(amount,level));
        return builder;
    }
    public static PotionBuilder WaterResistance(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.Resistence(ElementsEnum.Water,AmountWithLevel(amount,level));
        return builder;
    }
    public static PotionBuilder RockResistance(float amount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Timed,duration);
        builder.Resistence(ElementsEnum.Rock,AmountWithLevel(amount,level));
        return builder;
    }


    public static PotionBuilder TakeDamageAndResistOverTime(int damageAmount, ElementsEnum element, float resistenceAmount, float duration, int level){
        PotionBuilder builder = new PotionBuilder(PotionBuilder.PotionType.Periodic,duration);

        if(element == ElementsEnum.Fire) builder.Resistence(ElementsEnum.Fire,AmountWithLevel(resistenceAmount,level));
        else if(element == ElementsEnum.Water) builder.Resistence(ElementsEnum.Water,AmountWithLevel(resistenceAmount,level));
        else builder.Resistence(ElementsEnum.Rock,AmountWithLevel(resistenceAmount,level));;
        
        builder.TakeDamageOverTime(AmountWithLevel(damageAmount,level));
        return builder;
    }
}