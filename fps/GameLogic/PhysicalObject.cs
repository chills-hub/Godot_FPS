using GameLogic;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPS.GameLogic
{
    public partial class PhysicalObject : RigidBody3D, IInteractable
    {
        bool IInteractable.CanInteract { get; set; } = true;

        void IInteractable.DoSomething()
        {
            throw new NotImplementedException();
        }
    }
}
