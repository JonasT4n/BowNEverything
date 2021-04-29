using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNEGame
{
    public interface IInputHandler
    {
        bool IsLocalPlayer { get; }
        bool UseMobileControl { set; get; }
        bool InputLocked { set; }
        InputData LocalInputData { get; }
    }

}

