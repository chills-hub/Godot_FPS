using FPS.Managers;
using FPS.UILogic;
using Godot;
using System;
using System.Collections.Generic;
using UILogic;

public partial class UserInterfaceManager : Node
{
    public static UserInterfaceManager Instance { get; private set; }

    [Export]
    public Control InGameUI { get; set; }

    public PopupDialogueContainer PopupDialogueContainer { get; set; }

    public List<PlayerDialogueChoice> PlayerDialogueChoices { get; set; } = [];

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Instance = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

	public void MakePopupDialogue() 
	{
        PopupDialogueContainer = InGameUI.GetNode<PopupDialogueContainer>("PopupDialogueContainer");
        PopupDialogueContainer.DisplayedNPCText.Text = "This be a test gyarghh";
        PopupDialogueContainer.ContainerPlayerChoices.AddChild(new PlayerDialogueChoice());

        //PlayerDialogueChoices.Add(new PlayerDialogueChoice());
        //PlayerDialogueChoices.Add(new PlayerDialogueChoice());
        ////PlayerDialogueChoices[0].Text

        //for (int i = 0; i < PlayerDialogueChoices.Count; i++)
        //{
        //    PlayerDialogueChoice choice = PlayerDialogueChoices[i];
        //    PopupDialogueContainer.ContainerPlayerChoices.AddChild(choice);
        //}

        PopupDialogueContainer.Show();
    }
}
