using Godot;
using System;

public class PotionEffect{
    public Action<Player> whenApply;
    public Action whenStop;
    public Action whenUpdate;
    public Player player;
    protected Timer durationTimer, UpdateTimer;
    protected float duration = 3;
    protected bool isToStop = false;

    public PotionEffect(float duration) => this.duration = duration;
    public void Apply(Player player)
    {
        this.player = player;
        const string durationTimerName = "DurationPotionTimer";
        const string UpdateTimerName = "UpdatePotionTimer";

        if (player.HasNode(durationTimerName)) durationTimer = player.GetNode<Timer>(durationTimerName);
        else
        {
            durationTimer = new();
            durationTimer.Name = durationTimerName;
            player.AddChild(durationTimer);
        }
        durationTimer.Stop();
        durationTimer.WaitTime = duration;
        durationTimer.OneShot = true;
        durationTimer.Timeout += () =>
        {
            isToStop = true;
            durationTimer.Stop();
            UpdateTimer.Stop();
            whenStop?.Invoke();
        };
        if (player.HasNode(UpdateTimerName)) UpdateTimer = player.GetNode<Timer>(UpdateTimerName);
        else
        {
            UpdateTimer = new();
            UpdateTimer.Name = UpdateTimerName;
            player.AddChild(UpdateTimer);
        }
        UpdateTimer.Stop();
        UpdateTimer.WaitTime = 1f;
        UpdateTimer.Timeout += () =>
        {
            whenUpdate?.Invoke();
            if (!isToStop) UpdateTimer.Start();
        };
        whenApply?.Invoke(player);
        UpdateTimer.Start();
        durationTimer.Start();
    }
    public void Delete()
    {
        UpdateTimer.QueueFree();
        durationTimer.QueueFree();
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
    public PotionBuilder(float duration = 2f) => effect = new(duration);

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
        };
        return this;
    }
    
    public PotionBuilder TakeDamageAtEnd(int amount = 1, int level = 1){
        effect.whenStop += () => {
            effect.player.Damage(AmountWithLevel(amount,level));
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
    public PotionBuilder HealOverTime(int amount = 1, int level = 1)
    {
        effect.whenUpdate += () =>
        {
            effect.player.Heal(AmountWithLevel(amount, level));
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
    public PotionBuilder IncreaseDamage(float amount, int level = 1){
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
    public PotionBuilder Resistence(float amount, int level = 1)
    {
        effect.whenApply += (player) =>
        {
            player.potionModifier.defense = AmountWithLevel(amount, level);
        };
        effect.whenStop += () => { effect.player.potionModifier.defense = 0; };
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