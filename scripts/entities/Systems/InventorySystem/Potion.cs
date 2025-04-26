using Godot;
using System;

public interface IPotionEffect{
    void Apply(Player player);
    void Stop();
    void Update();
}

public class PeriodicEffect : IPotionEffect{
    protected float duration = 3;
    protected bool isToStop = false;
    Timer durationTimer, UpdateTimer;
    public virtual void Apply(Player player) {

        durationTimer = new();
        durationTimer.WaitTime = duration;
        durationTimer.OneShot = true;
        durationTimer.Timeout += Stop;

        UpdateTimer = new();
        UpdateTimer.WaitTime = 1f;
        UpdateTimer.Timeout += Update;

        player.AddChild(durationTimer);
        player.AddChild(UpdateTimer);
    }

    public virtual void Update(){
        if(isToStop) Stop();
    }

    public virtual void Stop(){
        isToStop = true;
        UpdateTimer.Stop();
    }
}


public class TimedEffect : IPotionEffect{
    protected float duration = 3;
    public virtual void Apply(Player player) {
        Timer timer = NodeFac.GenTimer(player, duration, Stop);
    }
    public virtual void Stop(){}
    public void Update(){}
}

public class InstaEffect : IPotionEffect{
    public virtual void Apply(Player player){

    }
    public void Stop(){}
    public void Update(){}
}

public class InstaHeal : InstaEffect{
    protected byte amount = 1;
    public override void Apply(Player player){
        base.Apply(player);
        player.Heal(amount);
    }
}