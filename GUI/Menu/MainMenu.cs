using Godot;
using System;

public partial class MainMenu : CenterContainer
{
    Configuracoes configuracoes;
    Timer openTime;
    bool canClose = false;
    public override void _Ready()
    {
        configuracoes = GetNode<Configuracoes>("Configuracoes");
        configuracoes.WhenDeactivate += () =>
        {
            Open();
        };

        GetNode<Button>("menu/Resume").Pressed += Resume;
        GetNode<Button>("menu/Configuração").Pressed += () =>
        {
            GetNode<VBoxContainer>("menu").Visible = false;
            configuracoes.Visible = true;
            canClose = false;
        };
        ProcessMode = ProcessModeEnum.Always;
        openTime = NodeMisc.GenTimer(this, 0.3f, () => { canClose = true; });
    }
    public void Open()
    {
        GetNode<VBoxContainer>("menu").Visible = true;
        openTime.Start();
        GetTree().Paused = true;
        Visible = true;
    }

    void Resume()
    {
        GetTree().Paused = false;
        Visible = false;
    }

    override public void _Input(InputEvent @event)
    {
        if (!Visible) return;
        if (@event is InputEventKey KeyEvent)
        {
            if (KeyEvent.Keycode == Key.Escape && canClose)
            {
                Resume();
            }
        }
    }
}
