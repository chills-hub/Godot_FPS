namespace UILogic;
using Godot;
using System;

public partial class PopupDialogueContainer : MarginContainer
{
	[Export]
	public VBoxContainer MainDialogueWindow { get; set; }

    [Signal]
    public delegate void SetPopupVisibilityEventHandler();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
       // MainDialogueWindow.Visible = false;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
