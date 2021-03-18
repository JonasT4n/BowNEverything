using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFactoryWithPool <T, U>
{
    void AddOrReplace(T bev, U type);

    /// <summary>
    /// This will dequeue a object from pool.
    /// </summary>
    T GetObjectRequired(U type);

    T CreateItem(U type, int amount);
}
