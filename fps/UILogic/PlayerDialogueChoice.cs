namespace FPS.UILogic;
using Godot;
using Godot.Collections;
using System;

public partial class PlayerDialogueChoice : Control
{
    public string Text { get; set; } = "Testing this feature...";

	public PlayerDialogueChoice() 
	{
		this.CustomMinimumSize = new Vector2(25, 0);
		this.SizeFlagsHorizontal = SizeFlags.ExpandFill;

		RichTextLabel label = new()
		{
			Text = Text,
            CustomMinimumSize = new Vector2(500, 0),
            Size = new Vector2(500,25)
        };

        this.AddChild(label);
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
