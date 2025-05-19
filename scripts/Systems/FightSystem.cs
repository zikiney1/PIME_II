
using System;
using Godot;

public class FightSystem
{
    public bool canAttack = true;
    public bool isInvencible = false;
    public float attackSpeed = 0.3f;
    public float invencibleTime = 0.5f;

    Timer attackTimer;
    Timer DamageTimer;

    public Action WhenStopAttack;

    public FightSystem(Node2D father, float attackSpeed, float invencibleTime = 0.5f)
    {
        this.attackSpeed = attackSpeed;
        this.invencibleTime = invencibleTime;

        attackTimer = NodeMisc.GenTimer(father, attackSpeed, () => { StopAtack(); });
        DamageTimer = NodeMisc.GenTimer(father, invencibleTime, () => {AfterDamageTimer(); });

    }
    public void Attack(float modifier=0)
    {
        float waitTime = attackSpeed + modifier;
        if(waitTime <= 0) waitTime = 0.01f;

        attackTimer.Stop();
        attackTimer.WaitTime = waitTime;
        attackTimer.Start();
        canAttack = false;
    }
    void StopAtack()
    {
        WhenStopAttack?.Invoke();
        canAttack = true;
    }
    public void GetDamage()
    {
        isInvencible = true;
        DamageTimer.Start();
    }

    void AfterDamageTimer()
    {
        isInvencible = false;
        DamageTimer.Stop();
    }
    

}