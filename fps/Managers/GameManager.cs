namespace FPS.Managers;
using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    [Export]
	public Player Player { get; set; }
	public Vector3 InteracteeLocation { get; set; }

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
