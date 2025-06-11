using Godot;
using System;

public partial class TelaCreditos : Control
{
    Timer creditsTimer;
    int timesToGoToMainMenu = 0;
    const int paraIrAoMenu = 25;
    AnimationPlayer porradaAnimationPlayer;
    RichTextLabel porradaText;
    const string porradaTexto = "x Tecladas para ir pro menu";
    bool canDarPorrada = false;
    public override void _Ready()
    {
        AnimationPlayer animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Play("creditos");
        creditsTimer = NodeMisc.GenTimer(this, (float)animationPlayer.CurrentAnimationLength, GoToMainMenu);
        porradaAnimationPlayer = GetNode<AnimationPlayer>("porradasPlayer");
        porradaText = GetNode<RichTextLabel>("porradaText");
        porradaText.Visible = false;
        Timer tm = NodeMisc.GenTimer(this, 0.5f, () => { canDarPorrada = true; });
        tm.Start();
    }

    void GoToMainMenu()
    {
        GetTree().ChangeSceneToFile("res://GUI/TelasPrincipais/MenuPrincipal.tscn");
    }
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion) return;
        Porrada();
        if (timesToGoToMainMenu > paraIrAoMenu)
        {
            GoToMainMenu();
        }
    }


    void Porrada()
    {
        if (!canDarPorrada) return;
        porradaText.Visible = true;
        timesToGoToMainMenu++;
        porradaAnimationPlayer.Stop();
        porradaAnimationPlayer.Play("porrada");
        porradaText.Text = $"{paraIrAoMenu-timesToGoToMainMenu} {porradaTexto}";
    }
}
