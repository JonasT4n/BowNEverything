using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICancelAbleAction
{
    bool IsCancelled { get; }

    void CancelAction(bool cancel);
}
