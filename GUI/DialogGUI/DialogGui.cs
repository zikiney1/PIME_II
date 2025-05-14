using Godot;
using System;

public partial class DialogGui : VBoxContainer
{
    RichTextLabel dialogText;
    RichTextLabel nameText;
    TextureRect portrait;

    Timer WordUpdater;
    int currentLetter = 0;
    bool endedAnimation = false;

    Player player;
    Sequencer<DialogResource> resourceSequence;
    Sequencer<string> DialogSequence;

    string EventAtEnd = "";

    public override void _Ready()
    {
        dialogText = GetNode<RichTextLabel>("DialogArea/PanelContainer/GridContainer/VBoxContainer/Dialog");
        nameText = GetNode<RichTextLabel>("RichTextLabel/Name");
        portrait = GetNode<TextureRect>("RichTextLabel/Portrait");
        player = GetNode<Player>("../.."); 

        dialogText.BbcodeEnabled = true;
        WordUpdater = NodeMisc.GenTimer(this, 0.025f, onWordUpdate);
        Deactivate();
    }


    /// <summary>
    /// Deactivates the dialog GUI, making it invisible and clearing the current dialog sequence.
    /// </summary>
    /// <remarks>
    /// This method sets the dialog GUI's visibility to false and nullifies the dialog sequence,
    /// effectively resetting the dialog state.
    /// </remarks>
    public void Deactivate(){
        Visible = false;
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
    public void Activate(DialogResource[] dialog,string EventAtEnd){
        Visible = true;
        if (dialog == null){
            player.InteractDialog(null,"");
            return;
        }
        this.EventAtEnd = EventAtEnd;
        resourceSequence = new(dialog);
        SetResource(resourceSequence.Current());
        
    }

    /// <summary>
    /// Sets the dialog GUI with the given dialog resource.
    /// </summary>
    /// <param name="resource">The dialog resource to set.</param>
    /// <remarks>
    /// This method updates the dialog sequence with the sequence from the given dialog resource,
    /// sets the name text to the character name from the resource, sets the portrait texture to the
    /// portrait from the resource, and sets the current dialog text to the first element of the
    /// sequence.
    /// </remarks>
    void SetResource(DialogResource resource){
        DialogSequence = new(resource.GetSequence());
        nameText.Text = resource.CharacterName;
        portrait.Texture = resource.portrait;
        SetDialog(DialogSequence.Current());
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
    void SetDialog(string dialog){
        currentLetter = 0;
        endedAnimation = false;

        dialogText.VisibleCharacters = 0;
        dialogText.Text = "";
        dialogText.Text = dialog;
        WordUpdater.Start();
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
        if(resourceSequence.isFinished && DialogSequence.isFinished){
            player.InteractDialog(null,"");
            EventHandler.EmitEvent(EventAtEnd);
        }else if(!DialogSequence.isFinished){
            SetDialog(DialogSequence.Next());
        }else{
            SetResource(resourceSequence.Next());
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible) return;
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.IsActionPressed("confirm"))
            {
                if (!endedAnimation){
                    cancelAnimation();
                }
                else{
                    Next();
                }
            }
        }
    }
}
