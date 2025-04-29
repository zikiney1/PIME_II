using Godot;
using System;

public class PotionEffect{
    public Action<Entitie> whenApply;
    public Action whenStop;
    public Action whenUpdate;
    public Entitie entitie;

    public virtual void Apply(Entitie entitie){
        this.entitie = entitie;
        whenApply?.Invoke(entitie);
    }
    public virtual void Delete(){}
}

public class PeriodicEffect : PotionEffect{
    public PeriodicEffect(float duration) => this.duration = duration;
    protected float duration = 3;
    protected bool isToStop = false;
    Timer durationTimer, UpdateTimer;

    public override void Apply(Entitie entitie) {
        base.Apply(entitie);

        const string durationTimerName = "DurationTimer";
        const string UpdateTimerName = "UpdateTimer";


        if(entitie.HasNode(durationTimerName)) durationTimer = entitie.GetNode<Timer>(durationTimerName);
        else {
            durationTimer = new();
            durationTimer.Name = durationTimerName;
            entitie.AddChild(durationTimer);
        }
        durationTimer.WaitTime = duration;
        durationTimer.OneShot = true;
        durationTimer.Timeout += whenStop;
        whenStop += () => {
            isToStop = true;
            durationTimer.Stop();
            UpdateTimer.Stop();
        };


        if(entitie.HasNode(UpdateTimerName)) UpdateTimer = entitie.GetNode<Timer>(UpdateTimerName);
        else {
            UpdateTimer = new();
            UpdateTimer.Name = UpdateTimerName;
            entitie.AddChild(UpdateTimer);
        }

        UpdateTimer.WaitTime = 1f;
        UpdateTimer.Timeout += whenUpdate;

        whenUpdate += () => {
            if(isToStop) whenStop?.Invoke();
        };
    }
    public override void Delete() {
        UpdateTimer.QueueFree();
        durationTimer.QueueFree();
    }
}


public class TimedEffect : PotionEffect{
    protected float duration = 3;
    public TimedEffect(float duration) => this.duration = duration;
    Timer timerEffect;
    public override void Apply(Entitie entitie) {
        const string timedEffectName = "TimedEffect";
        base.Apply(entitie);
        if(entitie.HasNode(timedEffectName)) timerEffect = entitie.GetNode<Timer>(timedEffectName);
        else timerEffect = NodeFac.GenTimer(entitie, duration, whenStop);

        timerEffect.Name = timedEffectName;
        entitie.AddChild(timerEffect);
    }
    public override void Delete(){
        timerEffect.QueueFree();
    }
}

public class InstaEffect : PotionEffect{
    public override void Apply(Entitie entitie){
        base.Apply(entitie);
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
            effect.entitie.Damage(AmountWithLevel(amount,level));
            effect.entitie.TakeDamageUpdate();
        };
        return this;
    }
    public PotionBuilder TakeDamageInstant(int amount = 1, int level = 1){
        effect.whenApply += (Entitie entitie) => {
            entitie.Damage(AmountWithLevel(amount,level));
            effect.entitie.WhenTakeDamage();
        };
        return this;
    }
    public PotionBuilder HealOverTime(int amount = 1, int level = 1){
        effect.whenUpdate += () => {
            effect.entitie.Heal(AmountWithLevel(amount,level));
            effect.entitie.HealUpdate();
        };
        return this;
    }
    public PotionBuilder HealInstant(int amount = 1, int level = 1){
        effect.whenApply += (Entitie entitie) => {
            entitie.Heal(AmountWithLevel(amount,level));
            effect.entitie.whenHeal();
        };
        return this;
    }
    
    public PotionBuilder Damage(ElementsEnum element, float amount, int level = 1){
        effect.whenApply += (entitie) => {
            entitie.entitieModifier.SetPotionDamageModifier((Half)AmountWithLevel(amount,level),element);  
        };
        effect.whenStop += () => effect.entitie.entitieModifier.ResetPotionDamageModifier(element);
        return this;
    }
    public PotionBuilder Resistence(ElementsEnum element, float amount, int level = 1){
        effect.whenApply += (entitie) => {
            entitie.entitieModifier.SetPotionResistenceModifier((Half)AmountWithLevel(amount,level),element);  
        };
        effect.whenStop += () => effect.entitie.entitieModifier.ResetPotionResistenceModifier(element);
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
        effect.whenApply += (entitie) => {
            entitie.entitieModifier.SetPotionSpeedModifier((Half)AmountWithLevel(amount,level));  
        };
        effect.whenStop += () => effect.entitie.entitieModifier.ResetPotionSpeedModifier();
        return this;
    }

    public PotionEffect Build() => effect;

}