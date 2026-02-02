using Godot;
using System;

public partial class InteractionManager : Node
{
    [Export] public RayCast3D InteractionRaycast {  get; set; }
    [Export] public Camera3D PlayerCamera { get; set; }
    [Export] public GodotObject CurrentObject { get; set; }
    //last thing we possibly could have interacted with
    [Export] public GodotObject LastPotentialObject { get; set; }
    //Node on the object that allows us to interact, will live on object
    [Export] public Node InteractionComponent { get; set; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        return;
	}
}
