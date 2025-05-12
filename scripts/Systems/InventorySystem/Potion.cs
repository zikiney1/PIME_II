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
    public virtual void Delete(){}
}

public class PeriodicEffect : PotionEffect{
    public PeriodicEffect(float duration) => this.duration = duration;
    protected float duration = 3;
    protected bool isToStop = false;
    Timer durationTimer, UpdateTimer;

    /// <summary>
    /// Apply this effect to the given player. This will start both the duration timer and the update timer.
    /// The duration timer will stop the effect when it timeouts, and the update timer will call the whenUpdate action every 1 second.
    /// </summary>
    /// <param name="player">The player to which to apply the effect.</param>
    public override void Apply(Player player) {
        base.Apply(player);

        const string durationTimerName = "DurationTimer";
        const string UpdateTimerName = "UpdateTimer";


        if(player.HasNode(durationTimerName)) durationTimer = player.GetNode<Timer>(durationTimerName);
        else {
            durationTimer = new();
            durationTimer.Name = durationTimerName;
            player.AddChild(durationTimer);
        }
        durationTimer.WaitTime = duration;
        durationTimer.OneShot = true;
        durationTimer.Timeout += whenStop;
        whenStop += () => {
            isToStop = true;
            durationTimer.Stop();
            UpdateTimer.Stop();
        };


        if(player.HasNode(UpdateTimerName)) UpdateTimer = player.GetNode<Timer>(UpdateTimerName);
        else {
            UpdateTimer = new();
            UpdateTimer.Name = UpdateTimerName;
            player.AddChild(UpdateTimer);
        }

        UpdateTimer.WaitTime = 1f;
        UpdateTimer.Timeout += whenUpdate;

        whenUpdate += () => {
            if(isToStop) whenStop?.Invoke();
        };
    }
        /// <summary>
        /// Stop the effect and free the timers.
        /// </summary>
    public override void Delete() {
        UpdateTimer.QueueFree();
        durationTimer.QueueFree();
    }
}


public class TimedEffect : PotionEffect{
    protected float duration = 3;
    public TimedEffect(float duration) => this.duration = duration;
    Timer timerEffect;

    /// <summary>
    /// Apply this timed effect to the given player. 
    /// This method sets up a timer that lasts for the specified duration, 
    /// and when the timer completes, it invokes the whenStop action.
    /// If a timer with the same name already exists, it reuses it.
    /// </summary>
    /// <param name="player">The player to which to apply the effect.</param>
    public override void Apply(Player player) {
        const string timedEffectName = "TimedEffect";
        base.Apply(player);
        if(player.HasNode(timedEffectName)) timerEffect = player.GetNode<Timer>(timedEffectName);
        else timerEffect = NodeMisc.GenTimer(player, duration, whenStop);

        timerEffect.Name = timedEffectName;
        player.AddChild(timerEffect);
    }
    public override void Delete(){
        timerEffect.QueueFree();
    }
}

public class InstaEffect : PotionEffect{
    public override void Apply(Player player){
        base.Apply(player);
    }
}

public class PotionBuilder{

    /// <summary>
    /// Returns the amount modified by the level.
    /// The returned value is the amount plus the amount divided by 5, times the level minus one, rounded to the nearest integer.
    /// </summary>
    /// <param name="amount">The amount to modify.</param>
    /// <param name="level">The level to use for the modification.</param>
    /// <returns>The modified amount.</returns>
    public static int AmountWithLevel(int amount,int level){
        return amount + (int)Math.Round((float)(amount/5) * (level-1));
    }
    /// <summary>
    /// Calculates the modified amount based on the given level.
    /// The modification is calculated by adding the original amount to the product of the amount divided by 5 and the level minus one, rounded to the nearest integer.
    /// </summary>
    /// <param name="amount">The base amount to modify.</param>
    /// <param name="level">The level used to determine the modification.</param>
    /// <returns>The modified amount as a float.</returns>
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

    /// <summary>
    /// Applies a damage over time effect to the player.
    /// This effect reduces the player's health periodically by a calculated amount
    /// based on the given parameters. The damage applied is determined by the
    /// base amount and is adjusted according to the player's level.
    /// </summary>
    /// <param name="amount">The base amount of damage to apply over time.</param>
    /// <param name="level">The level used to modify the damage amount.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder TakeDamageOverTime(int amount = 1, int level = 1){
        effect.whenUpdate += () => {
            effect.player.Damage(AmountWithLevel(amount,level));
            effect.player.TakeDamageUpdate();
        };
        return this;
    }

    /// <summary>
    /// Applies a damage effect to the player once when the potion is applied.
    /// The damage applied is determined by the base amount and is adjusted according to the player's level.
    /// </summary>
    /// <param name="amount">The base amount of damage to apply.</param>
    /// <param name="level">The level used to modify the damage amount.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder TakeDamageInstant(int amount = 1, int level = 1){
        effect.whenApply += (Player player) => {
            player.Damage(AmountWithLevel(amount,level));
            effect.player.WhenTakeDamage();
        };
        return this;
    }
    
    /// <summary>
    /// Applies a health over time effect to the player.
    /// This effect restores the player's health periodically by a calculated amount
    /// based on the given parameters. The heal amount applied is determined by the
    /// base amount and is adjusted according to the player's level.
    /// </summary>
    /// <param name="amount">The base amount of health to restore over time.</param>
    /// <param name="level">The level used to modify the heal amount.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder HealOverTime(int amount = 1, int level = 1){
        effect.whenUpdate += () => {
            effect.player.Heal(AmountWithLevel(amount,level));
            effect.player.HealUpdate();
        };
        return this;
    }
    
    /// <summary>
    /// Applies an instant heal effect to the player once when the potion is applied.
    /// The heal amount applied is determined by the base amount and is adjusted according to the player's level.
    /// </summary>
    /// <param name="amount">The base amount of health to restore instantly.</param>
    /// <param name="level">The level used to modify the heal amount.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder HealInstant(int amount = 1, int level = 1){
        effect.whenApply += (Player player) => {
            player.Heal(AmountWithLevel(amount,level));
            effect.player.whenHeal();
        };
        return this;
    }
    
    /// <summary>
    /// Applies a temporary damage modifier to the player when the potion is applied.
    /// The damage modifier is calculated based on the given base amount and the player's level.
    /// The modifier is removed when the effect stops.
    /// </summary>
    /// <param name="amount">The base amount of damage to modify.</param>
    /// <param name="level">The level used to modify the damage amount. Defaults to 1.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder Damage(float amount, int level = 1){
        effect.whenApply += (player) => {
            player.potionModifier.damage = AmountWithLevel(amount,level);
        };
        effect.whenStop += () => {effect.player.potionModifier.damage = 0;};
        return this;
    }
    /// <summary>
    /// Applies a temporary defense modifier to the player when the potion is applied.
    /// The defense modifier is calculated based on the given base amount and the player's level.
    /// The modifier is removed when the effect stops.
    /// </summary>
    /// <param name="amount">The base amount of defense to modify.</param>
    /// <param name="level">The level used to modify the defense amount. Defaults to 1.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder Resistence(float amount, int level = 1){
        effect.whenApply += (player) => {
            player.potionModifier.defense = AmountWithLevel(amount,level);  
        };
        effect.whenStop += () => {effect.player.potionModifier.defense = 0;};
        return this;
    }
    /// <summary>
    /// Applies a temporary speed modifier to the player when the potion is applied.
    /// The speed modifier is calculated based on the given base amount and the player's level.
    /// The modifier is removed when the effect stops.
    /// </summary>
    /// <param name="amount">The base amount of speed to modify.</param>
    /// <param name="level">The level used to modify the speed amount. Defaults to 1.</param>
    /// <returns>The PotionBuilder instance to allow for method chaining.</returns>
    public PotionBuilder Speed(float amount, int level = 1){
        effect.whenApply += (player) => {
            player.potionModifier.speed = AmountWithLevel(amount,level); 
        };
        effect.whenStop += () => effect.player.potionModifier.speed = 0;
        return this;
    }

    public PotionEffect Build() => effect;

}

public class PotionModifier{
    public float speed = 0;
    public float attackSpeed = 0;
    public float defense = 0;
    public float damage = 0;
}