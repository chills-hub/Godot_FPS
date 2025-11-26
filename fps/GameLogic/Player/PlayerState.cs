namespace FPS.GameLogic.Player;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum PlayerState
{
    Normal = 0,
    Inventory = 1,
    Conversation = 2,
    HeldObject = 3,
    None = 4, //do nothing
}

