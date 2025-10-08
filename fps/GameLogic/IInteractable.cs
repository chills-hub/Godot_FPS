using FPS.GameLogic.Player;
using Godot;

namespace GameLogic;
public interface IInteractable
{
    public bool CanInteract { get; set; }

   /// <summary>
   /// public abstract PlayerState PlayerStateEffect { get; set; }
   /// </summary>

    public void DoSomething();
}

