using Godot;
using System;

public partial class PlayerAudioManager : Node
{
    
    public static PlayerAudioManager Instance;

    public enum SongToPlay
    {
        Overworld,
        Boss,
        VilaDoMar,
        Anhau
    }
    AudioStreamPlayer2D songPlayer;
    AudioStreamPlayer2D sfxPlayer;
    AudioStreamPlayer2D walkPlayer;

    //musicas
    [ExportGroup("Music")]
    [Export] AudioStream overworld;
    [Export] AudioStream boss;
    [Export] AudioStream VilaDoMar;
    [Export] AudioStream anhau;

    //sons
    [ExportGroup("SFX")]
    [Export] AudioStream dialogSound;
    [Export] AudioStream AttackSound;
    [Export] AudioStream ZarabatanSound;
    [Export] AudioStream HurtSound;


    [ExportCategory("walk variations")]
    [Export] AudioStream walkSound1;
    [Export] AudioStream walkSound2;
    [Export] AudioStream walkSound3;

    int walkIndex = 0;
    Random rnd = new();

    public override void _EnterTree()
    {
        base._EnterTree();
        Instance = this;
    }

    public override void _Ready()
    {
        songPlayer = GetNode<AudioStreamPlayer2D>("SongPlayer");
        sfxPlayer = GetNode<AudioStreamPlayer2D>("SFXPlayer");
        walkPlayer = GetNode<AudioStreamPlayer2D>("WalkPlayer");

    }
    public void PlayDialog() => PlaySFX(dialogSound);
    public void PlayAttack() => PlaySFX(AttackSound);
    public void PlayZarabatan() => PlaySFX(ZarabatanSound);
    public void PlayHurt() => PlaySFX(HurtSound);

    void PlaySFX(AudioStream stream)
    {
        sfxPlayer.Stream = stream;
        sfxPlayer.Play();
    }
    public void PlayWalk() {
        if(walkPlayer.IsPlaying()) return;

        walkIndex = rnd.Next(0, 3);

        switch (walkIndex)
        {
            case 0:
                walkPlayer.Stream = walkSound1;
                break;
            case 1:
                walkPlayer.Stream = walkSound2;
                break;
            case 2:
                walkPlayer.Stream = walkSound3;
                break;
        }
        walkPlayer.Play();
    }
    public void StopWalk()
    {
        walkPlayer.Stop();
    }

    public void StopSFX() => sfxPlayer.Stop();

    public void PlaySong(SongToPlay song)
    {
        switch (song)
        {
            case SongToPlay.Overworld:
                songPlayer.Stream = overworld;
                break;
            case SongToPlay.Boss:
                songPlayer.Stream = boss;
                break;
            case SongToPlay.VilaDoMar:
                songPlayer.Stream = VilaDoMar;
                break;
            case SongToPlay.Anhau:
                songPlayer.Stream = anhau;
                break;
        }
        songPlayer.Play();
    }
}
