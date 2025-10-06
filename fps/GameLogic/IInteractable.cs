using Godot;

namespace GameLogic;
public interface IInteractable
{
    public bool CanInteract { get; set; }
    public void DoSomething();
}

