using FPS.Managers;
using Godot;
using System;

public partial class UserInterfaceManager : Node
{
    public static UserInterfaceManager Instance { get; private set; }

    [Export]
    public Control InGameUI { get; set; }

	// Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Instance = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
