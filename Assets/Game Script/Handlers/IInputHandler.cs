using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputHandler
{
    bool IsLocalPlayer { get; }
    bool UseMobileControl { set; get; }
    bool InputLocked { set; }
    InputData LocalInputData { get; }
}
