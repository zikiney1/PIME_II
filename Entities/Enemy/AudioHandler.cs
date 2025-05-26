using Godot;
using System;
using System.Collections.Generic;

public partial class AudioHandler : AudioStreamPlayer2D
{
    [Export] AudioStream hitted;
    [Export] AudioStream walk;
    [Export] AudioStream die;
    [Export] AudioStream shoot;
    [Export] AudioStream[] specialSFX;

    AudioStreamPlayer2D audioPlayer;

    public override void _Ready()
    {
        base._Ready();
        MaxDistance = 10000;
    }

    public void PlaySFX(AudioStream stream)
    {
        Stop();
        Stream = stream;
        Play();
    }
    public void PlayLoopSFX(AudioStream stream)
    {
        if(IsPlaying()) return;
        Stream = stream;
        Play();
    }

    public void PlaySpecialSFX(int index, bool isLoop = false)
    {
        if(specialSFX == null) return;
        if(specialSFX.Length <= index || specialSFX[index] == null || specialSFX.Length == 0) return;
        if (isLoop) PlayLoopSFX(specialSFX[index]);
        else PlaySFX(specialSFX[index]);
    }

    public void PlayHit() => PlaySFX(hitted);
    public void PlayWalk() => PlayLoopSFX(walk);
    public void PlayDie() => PlaySFX(die);
    public void PlayShoot() => PlaySFX(shoot);

}
