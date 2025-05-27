using Godot;
using System;

public partial class MainMenu : CenterContainer
{
    Configuracoes configuracoes;
    Timer openTime;
    bool canClose = false, canOpen = true;
    Action openOrClose;
    public override void _Ready()
    {
        configuracoes = GetNode<Configuracoes>("Configuracoes");
        configuracoes.WhenDeactivate += () =>
        {
            Open();
        };

        GetNode<Button>("menu/Resume").Pressed += Resume;
        GetNode<Button>("menu/Main Menu").Pressed += GameManager.Instance.GoToMainMenu;
        GetNode<Button>("menu/Configuração").Pressed += () =>
        {
            GetNode<VBoxContainer>("menu").Visible = false;
            configuracoes.Visible = true;
            canClose = false;
        };
        ProcessMode = ProcessModeEnum.Always;
        openTime = NodeMisc.GenTimer(this, 0.35f, () => { openOrClose?.Invoke(); });
        openTime.ProcessMode = ProcessModeEnum.Always;
    }
    public void Open()
    {
        if (!canOpen) return;

        Player.Instance.ActivateBlur();
        GetTree().Paused = true;
        
        Visible = true;
        GetNode<VBoxContainer>("menu").Visible = true;

        canClose = false;
        openTime.Stop();
        openOrClose = () => canClose = true;
        openTime.Start();
    }

    void Resume()
    {
        if (!canClose) return;
        Player.Instance.DeactivateBlur();
        GetTree().Paused = false;
        Visible = false;

        canOpen = false;
        openTime.Stop();
        openOrClose = () => canOpen = true;
        openTime.Start();
        
    }

    override public void _Input(InputEvent @event)
    {
        if (!Visible) return;
        if (@event is InputEventKey KeyEvent)
        {
            if (KeyEvent.Keycode == Key.Escape)
            {
                Resume();
            }
        }
    }
}
