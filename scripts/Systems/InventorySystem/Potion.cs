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
    public static int AmountWithLevel(int amount,int level){
        return amount + (int)Math.Round((float)(amount/5) * (level-1));
    }
    public static float AmountWithLevel(float amount,int level){
        return (float)(amount + Math.Round((amount/5) * (level-1)));
    }
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

    public PotionBuilder TakeDamageOverTime(int amount = 1, int level = 1){
        effect.whenUpdate += () => {
            effect.player.Damage(AmountWithLevel(amount,level));
        };
        return this;
    }
    public PotionBuilder TakeDamageInstant(int amount = 1, int level = 1){
        effect.whenApply += (Player player) => {
            player.Damage(AmountWithLevel(amount,level));
        };
        return this;
    }
    public PotionBuilder HealOverTime(int amount = 1, int level = 1){
        effect.whenUpdate += () => {
            effect.player.Heal(AmountWithLevel(amount,level));
        };
        return this;
    }
    public PotionBuilder HealInstant(int amount = 1, int level = 1){
        effect.whenApply += (Player player) => {
            player.Heal(AmountWithLevel(amount,level));
        };
        return this;
    }
    
    public PotionBuilder Damage(ElementsEnum element, float amount, int level = 1){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionDamageModifier((Half)AmountWithLevel(amount,level),element);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionDamageModifier(element);
        return this;
    }
    public PotionBuilder Resistence(ElementsEnum element, float amount, int level = 1){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionResistenceModifier((Half)AmountWithLevel(amount,level),element);  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionResistenceModifier(element);
        return this;
    }
    public PotionBuilder Resistence(Element element, float amount, int level = 1){
        foreach (ElementsEnum elementType in element.Weaknesses()){
            Resistence(elementType,AmountWithLevel(-amount,level));
        }
        foreach (ElementsEnum elementType in element.Resistances()){
            Resistence(elementType,AmountWithLevel(amount,level));
        }
        return this;
    }
    public PotionBuilder Speed(float amount, int level = 1){
        effect.whenApply += (player) => {
            player.entitieModifier.SetPotionSpeedModifier((Half)AmountWithLevel(amount,level));  
        };
        effect.whenStop += () => effect.player.entitieModifier.ResetPotionSpeedModifier();
        return this;
    }

    public PotionEffect Build() => effect;

}