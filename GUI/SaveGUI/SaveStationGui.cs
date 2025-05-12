using Godot;
using System;
using System.Collections.Generic;

public partial class SaveStationGui : CenterContainer
{
    public Dictionary<string,int> checkPoints = new();
    public List<SaveStation> checkPointList = new();
    public Player player;

    Button SaveButton;
    Button ExitButton;

    
    Button SelectedStation;
    int selectedIndex = 0;
    Button Left;
    Button Right;

    int focusedIndex = 0;

    public override void _Ready()
    {
        player = GetNode<Player>("../..");
        SaveButton = GetNode<Button>("PanelContainer/VBoxContainer/Salvar");
        ExitButton = GetNode<Button>("PanelContainer/VBoxContainer/Sair");

        Left = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/Left");
        SelectedStation = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/Selected");
        Right = GetNode<Button>("PanelContainer/VBoxContainer/HBoxContainer/Right");

        Left.Pressed += () => {
            changeIndex(-1);
        };
        Right.Pressed += () => {
            changeIndex(1);
        };
        ExitButton.Pressed += () => {
            player.InteractSaveStation(null);
        };
        SelectedStation.Pressed += Teleport;
        SaveButton.Pressed += Save;
        Deactivate();
    }

    /// <summary>
    /// Retrieves the names of all checkpoints in the save station GUI.
    /// </summary>
    /// <returns>An array of checkpoint names.</returns>
    public string[] ToNames(){
        string[] names = new string[checkPointList.Count];
        for(int i = 0; i < checkPointList.Count; i++){
            names[i] = checkPointList[i].Name;
        }
        return names;
    }
    
    /// <summary>
    /// Changes the selected index in the save station GUI by the given index,
    /// wrapping around to the start or end of the list if the given index is
    /// out of range.
    /// </summary>
    /// <param name="index">The index to change the selected index by.</param>
    void changeIndex(int index = 0){
        selectedIndex += index;
        if(selectedIndex >= checkPointList.Count){
            selectedIndex = 0;
        }else if(selectedIndex < 0){
            selectedIndex = checkPointList.Count-1;
        }
        SelectedStation.Text = checkPointList[selectedIndex].Name;
    }

    
        /// <summary>
        /// Activates the save station GUI, adding the given save station, making the GUI visible, setting the focus to the Save button, and setting the selected index to 0.
        /// </summary>
        /// <param name="saveStation">The save station to add.</param>
    public void Activate(SaveStation saveStation){
        AddStation(saveStation);
        Visible = true;
        SaveButton.GrabFocus();
        focusedIndex = 0;
        selectedIndex = 0;
        UpdateName();
    }
    public void Deactivate(){
        Visible = false;
    }

    public void UpdateName(){
        SelectedStation.Text = checkPointList[selectedIndex].Name;
    }


    void Save(){
        player.Save(checkPointList[selectedIndex].savePos);
        player.InteractSaveStation(null);
        
    }
    void Teleport(){
        player.Teleport(checkPointList[selectedIndex].savePos);
        Deactivate();
    }
    public void AddStation(SaveStation saveStation){
        if(!checkPoints.ContainsKey(saveStation.Name)){
            checkPointList.Add(saveStation);
            checkPoints.Add(saveStation.Name,checkPointList.Count-1);
        }
    }

        /// <summary>
        /// Handles the input events for the save station GUI. If the GUI is not visible, the function does nothing.
        /// 
        /// If the input is a key event, it handles the following cases:
        /// <list type="bullet">
        /// <item>Up/Down keys: changes the focus between the save and exit buttons</item>
        /// <item>Left/Right keys and the focus is on the selected station: changes the selected station</item>
        /// </list>
        /// </summary>
    public override void _Input(InputEvent @event)
    {
        if(!Visible) return;
        if(@event is InputEventKey KeyEvent){

            Vector2 dir = InputSystem.GetVector();

            focusedIndex += (int)dir.Y;

            if(dir.Y != 0){
                if(focusedIndex < 0) focusedIndex = 2;
                if(focusedIndex > 2) focusedIndex = 0;
                switch(focusedIndex){
                    case 0:
                        SaveButton.GrabFocus();
                        break;
                    case 1:
                        SelectedStation.GrabFocus();
                        break;
                    case 2:
                        ExitButton.GrabFocus();
                        break;
                }
            }

            if(dir.X != 0){
                if(focusedIndex != 1) return;
                changeIndex((int)dir.X);
            }


        }
    }

}
