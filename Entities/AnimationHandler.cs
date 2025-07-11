using Godot;
public class AnimationHandler
{
    public AnimationPlayer characterAnimPlayer;
    public AnimationPlayer hitAnimPlayer;


    public AnimationHandler(AnimationPlayer characterAnimPlayer, AnimationPlayer hitAnimPlayer)
    {
        this.characterAnimPlayer = characterAnimPlayer;
        this.hitAnimPlayer = hitAnimPlayer;
    }

    public void SetVel(float vel)
    {
        characterAnimPlayer.SpeedScale = vel;
    }
    public void StopCharacter()
    {
        characterAnimPlayer.Stop();
    }
    public void StopHit()
    {
        hitAnimPlayer.Stop();
    }
    public void Direction(Vector2 direction)
    {

        if (direction.Y < 0)
            characterAnimPlayer.Play("walk_up");
        else if (direction.Y > 0)
            characterAnimPlayer.Play("walk_down");
        else if (direction.X > 0)
            characterAnimPlayer.Play("walk_right");
        else if (direction.X < 0)
            characterAnimPlayer.Play("walk_left");
        else
            characterAnimPlayer.Play("idle");
    }

    public void Defend(Vector2 direction)
    {
        if (direction.Y < 0)
            characterAnimPlayer.Play("defend_up");
        else if (direction.Y > 0)
            characterAnimPlayer.Play("defend_down");
        else if (direction.X > 0)
            characterAnimPlayer.Play("defend_right");
        else if (direction.X < 0)
            characterAnimPlayer.Play("defend_left");
    }

    public void Attack(Vector2 direction)
    {
        if (direction.Y < 0)
            characterAnimPlayer.Play("attack_up");
        else if (direction.Y > 0)
            characterAnimPlayer.Play("attack_down");
        else if (direction.X > 0)
            characterAnimPlayer.Play("attack_right");
        else if (direction.X < 0)
            characterAnimPlayer.Play("attack_left");
    }

    public void Play(string animationName)
    {
        if (characterAnimPlayer.HasAnimation(animationName))
            characterAnimPlayer.Play(animationName);
        else if (hitAnimPlayer.HasAnimation(animationName))
            hitAnimPlayer.Play(animationName);
        else
            GD.PushError("Animation " + animationName + " not found");
    }

    public void Attack2(Vector2 direction)
    {
        if (direction.Y < 0)
            characterAnimPlayer.Play("attack2_up");
        else if (direction.Y > 0)
            characterAnimPlayer.Play("attack2_down");
        else if (direction.X > 0)
            characterAnimPlayer.Play("attack2_right");
        else if (direction.X < 0)
            characterAnimPlayer.Play("attack2_left");
    }

    public double GetAnimationTime()
    {
        if (characterAnimPlayer.IsPlaying())
            return characterAnimPlayer.CurrentAnimationLength;
        else if (hitAnimPlayer.IsPlaying())
            return hitAnimPlayer.CurrentAnimationLength;
        else
            return 0;
    }

    public void Damage()
    {
        hitAnimPlayer.Play("take_damage");
    }

    public void Die()
    {
        characterAnimPlayer.Play("die");
    }

}