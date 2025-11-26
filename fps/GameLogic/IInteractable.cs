using FPS.GameLogic;
using FPS.GameLogic.Player;
using Godot;

namespace GameLogic;
public interface IInteractable
{
    public bool CanInteract { get; set; }
    public bool CanLift { get; set; }
    public InteractionType InteractionType { get; set; }

    /// <summary>
    /// public abstract PlayerState PlayerStateEffect { get; set; }
    /// </summary>

    public void DoInteraction();
}

