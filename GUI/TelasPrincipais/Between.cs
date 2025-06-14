using Godot;
using System;

public partial class Between : CanvasLayer
{
    Timer timerAnimation;
    AnimationPlayer animationPlayer;
    Action whenChanged;
    public static Between instance;
    public override void _Ready()
    {
        if (instance != null) return;
        instance = this;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        timerAnimation = new();
        AddChild(timerAnimation);

        timerAnimation = NodeMisc.GenTimer(this, 0, () =>
        {
            whenChanged?.Invoke();
            animationPlayer.PlayBackwards("dissolve");
        });
    }


    public void ChangeScene(string scene)
    {
        animationPlayer.Play("dissolve");
        whenChanged = () => GetTree().ChangeSceneToFile(scene);
        timerAnimation.WaitTime = (float)animationPlayer.CurrentAnimationLength;
        timerAnimation.Start();
    }
}
