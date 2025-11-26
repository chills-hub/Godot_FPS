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
        bool IInteractable.CanInteract { get; set; } = false;
        bool IInteractable.CanLift { get; set; } = true; //test value
        public InteractionType InteractionType { get; set; } = InteractionType.Pickup;

        void IInteractable.DoInteraction()
        {
            throw new NotImplementedException();
        }
    }
}
