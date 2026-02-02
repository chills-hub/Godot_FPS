using Godot;
using System;
/// <summary>
/// Used on individual object ingame
/// </summary>
public partial class InteractionComponent : Node
{
    public enum ObjectInteractionType
    {
        None,
        Talk,
        Pickup
    }

    [Export] public Node3D ObjectRef { get; set; }
    [Export] public ObjectInteractionType InteractionType { get; set; } = ObjectInteractionType.None;  //(for now)

}
