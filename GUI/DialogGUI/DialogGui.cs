using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DialogGui : VBoxContainer
{
    RichTextLabel dialogText;
    RichTextLabel nameText;
    TextureRect portrait;
    AnimationPlayer animationPlayer;

    Timer WordUpdater;
    Timer animationTimer;
    int currentLetter = 0;
    bool endedAnimation = false;

    Player player;
    Sequencer<string> DialogSequence;

    Dictionary<string, Character> Characters = new();

    string EventAtEnd = "";
    Action animationTimerEnds;
    public bool isPlayingAnimation => animationPlayer.IsPlaying();
    bool isFinished = true;

    public override void _EnterTree()
    {
        dialogText = GetNode<RichTextLabel>("DialogArea/PanelContainer/MarginContainer/Dialog");
        nameText = GetNode<RichTextLabel>("RichTextLabel/Name");
        portrait = GetNode<TextureRect>("RichTextLabel/Portrait");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        player = GetNode<Player>("../..");

        dialogText.BbcodeEnabled = true;
        WordUpdater = NodeMisc.GenTimer(this, 0.025f, onWordUpdate);

        animationTimer = NodeMisc.GenTimer(this, 0.5f, () =>
        {
            animationTimerEnds?.Invoke();
            animationPlayer.Stop(true);
        });
        Deactivate(false);
        SetupCharacters();
    }


    public void SetupCharacters()
    {
        Character tutorial = new("", "");
        Character arana = new("Aranã", "res://Dialogo/portrait/arana.png");
        Character PaiDeArana = new("Pai de Arana", "res://Dialogo/portrait/pai_de_arana.png");
        Character apoena = new("Apoena", "res://Dialogo/portrait/apoena.png");
        Character apua = new("Apuã", "res://Dialogo/portrait/apua.png");
        Character karai = new("Karai", "res://Dialogo/portrait/karai.png");
        Character luna = new("Velha Luna", "res://Dialogo/portrait/luna.png");
        Character caua = new("Cauã", "res://Dialogo/portrait/caua.png");
        Character thauan = new("Thauan", "res://Dialogo/portrait/thauan.png");
        Character moacir = new("Moacir", "res://Dialogo/portrait/moacir.png");
        Character espirito = new("Moacir", "res://Dialogo/portrait/espirito.png");

        Characters.Add("tutorial", tutorial);
        Characters.Add("arana", arana);
        Characters.Add("pai_de_arana", PaiDeArana);
        Characters.Add("apoena", apoena);
        Characters.Add("apua", apua);
        Characters.Add("karai", karai);
        Characters.Add("luna", luna);
        Characters.Add("caua", caua);
        Characters.Add("thauan", thauan);
        Characters.Add("moacir", moacir);
        Characters.Add("espirito", espirito);
    }

    /// <summary>
    /// Deactivates the dialog GUI, making it invisible and clearing the current dialog sequence.
    /// </summary>
    /// <remarks>
    /// This method sets the dialog GUI's visibility to false and nullifies the dialog sequence,
    /// effectively resetting the dialog state.
    /// </remarks>
    public void Deactivate(bool playAnimation = true)
    {
        if (playAnimation)
        {
            animationPlayer.Play("close");
            animationTimer.WaitTime = animationPlayer.CurrentAnimationLength;
            animationTimer.Start();
            animationTimerEnds = () => { Visible = false; };
        }
        else
        {
            Visible = false;
        }
        DialogSequence = null;
    }

    /// <summary>
    /// Activates the dialog GUI with the provided dialog resources.
    /// </summary>
    /// <param name="dialog">An array of dialog resources to be displayed. If null, interaction with the player is terminated.</param>
    /// <remarks>
    /// This method makes the dialog GUI visible and initializes the resource sequence with the provided dialog resources. 
    /// If the dialog is null, it signals the player to end the interaction.
    /// </remarks>
    public void Activate(string DialogSequencePath,string EventAtEnd,bool playAnimation = true){
        dialogText.Text = "";
        portrait.Texture = null;
        nameText.Text = "";
        if (DialogSequencePath == null || DialogSequencePath == "")
        {
            player.InteractDialog(null, "");
            return;
        }
        Visible = true;
        if (playAnimation)
        {
            animationPlayer.Play("open");
            animationTimerEnds = () =>{ SetDialog(DialogSequencePath, EventAtEnd); };
            animationTimer.WaitTime = animationPlayer.CurrentAnimationLength;
            animationTimer.Start();
        }
        else
        {
            SetDialog(DialogSequencePath, EventAtEnd);
        }
        
    }

    public void SetDialog(string dialogFile, string EventAtEnd)
    {
        DialogSequence = new(GetSequence(dialogFile));
        SetDialog(DialogSequence.Current());
        this.EventAtEnd = EventAtEnd;
        isFinished = false;
    }



    /// <summary>
    /// Sets the dialog GUI with the given dialog string.
    /// </summary>
    /// <param name="dialog">The dialog string to set.</param>
    /// <remarks>
    /// This method updates the current dialog text with the given string and starts the word update
    /// timer, which updates the visible characters of the text every frame. The visible characters
    /// are set to 0 to start the animation from the beginning. The ended animation flag is set to false
    /// to indicate that the animation has not finished yet.
    /// </remarks>
    void SetDialog(string dialogRaw){
        currentLetter = 0;
        endedAnimation = false;
        string[] splited = dialogRaw.Split(":");
        string dialog;
        if (splited.Length < 2){
            dialog = dialogRaw;
        } else {
            SetCharacter(Characters[splited[0]]);
            dialog = dialogRaw.Split(":")[1];
        }
        dialog.Trim();

        dialogText.VisibleCharacters = 0;
        dialogText.Text = "";
        dialogText.Text = dialog;
        WordUpdater.Start();
    }

    void SetCharacter(Character ch)
    {
        nameText.Text = ch.name;
        portrait.Texture = ch.portrait;   
    }

    /// <summary>
    /// Updates the dialog text visible characters by one character each frame,
    /// creating a typing animation effect.
    /// </summary>
    /// <remarks>
    /// This method is called every frame by the WordUpdater timer.
    /// It checks if the animation has finished (the current letter is greater than or equal to
    /// the total character count of the dialog text) and if so, sets the ended animation flag to true.
    /// If not, it increments the current letter and sets the visible characters of the dialog text to
    /// the current letter, and restarts the WordUpdater timer.
    /// </remarks>
    void onWordUpdate(){
        int totalChars = dialogText.GetTotalCharacterCount();
        if (currentLetter >= totalChars){
            endedAnimation = true;
            return;
        }

        currentLetter++;
        dialogText.VisibleCharacters = currentLetter;
        WordUpdater.Start();
    }

    /// <summary>
    /// Cancels the current word update animation and sets the dialog text visible characters to the total
    /// count of characters in the text.
    /// </summary>
    void cancelAnimation()
    {
        WordUpdater.Stop();
        dialogText.VisibleCharacters = dialogText.GetTotalCharacterCount();
        endedAnimation = true;
    }


    /// <summary>
    /// Advances the dialog or resource sequence to the next element, and updates the player's interaction state accordingly.
    /// </summary>
    /// <remarks>
    /// If both the resource and dialog sequences are finished, it ends the player's dialog interaction.
    /// Otherwise, it progresses the dialog sequence if it is not finished, or the resource sequence if the dialog is finished.
    /// </remarks>
    void Next(){
        if(DialogSequence == null) return;
        if (DialogSequence.isFinished)
        {
            isFinished = true;
            if (EventAtEnd != "") EventHandler.EmitEvent(EventAtEnd);
            if(isFinished) player.InteractDialog("", "");
        }
        else
        {
            SetDialog(DialogSequence.Next());
        }
    }
    public string[] GetSequence(string dialogPath){
        if(FileAccess.FileExists(dialogPath) == false) return null;
        FileAccess file = FileAccess.Open(dialogPath, FileAccess.ModeFlags.Read);
        string[] DialogSequence = file.GetAsText()
                                .Replace("\r","")
                                .Split('\n')
                                .Where(x => x != "")
                                .ToArray();
        file.Close();
        return DialogSequence;
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible) return;
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.IsActionPressed("confirm"))
            {
                if (!endedAnimation)
                {
                    cancelAnimation();
                }
                else
                {
                    Next();
                }
            }
        }
    }
}


public class Character
{
    public string name;
    public Texture2D portrait;
    public Character(string name, string portraitPath)
    {
        this.name = name;
        if(portraitPath == "") return;
        try{
            this.portrait = ResourceLoader.Load<Texture2D>(portraitPath);
        }catch(Exception e){
            GD.PushError(e);
        }
    }
}
