using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArrowTypes
{
    None = 0,
    Normal = 1,
    ChopStick = 2
    //Bounce = 2,
    //Glue = 3,
    //IceBlade = 4,
    //Anvil = 5,
    //FireCracker = 6,
    //Dynamite = 7,
    //MathBook = 8
}

public enum GameRarity
{
    Trash = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3
}

public class ArrowFactory : IFactoryWithPool<ArrowBehaviour, ArrowTypes>
{
    private const int MAX_EACH_POOL = 20;

    private Dictionary<ArrowTypes, ArrowBehaviour> _prefabs;
    private Transform _poolContainer;

    private Dictionary<ArrowTypes, Queue<ArrowBehaviour>> _pool = new Dictionary<ArrowTypes, Queue<ArrowBehaviour>>();

    public ArrowFactory(Dictionary<ArrowTypes, ArrowBehaviour> prefabs, Transform container = null)
    {
        _prefabs = prefabs;
        _poolContainer = container;

        foreach (ArrowTypes a in _prefabs.Keys)
        {
            if (a == ArrowTypes.None)
                continue;

            ArrowBehaviour pref = _prefabs[a];
            _pool.Add(a, new Queue<ArrowBehaviour>());

            for (int i = 0; i < MAX_EACH_POOL; i++)
            {
                ArrowBehaviour dupe = Object.Instantiate(pref);
                dupe.gameObject.SetActive(false);

                if (_poolContainer != null)
                    dupe.transform.parent = _poolContainer;
                _pool[a].Enqueue(dupe);
            }
        }
    }

    public void AddOrReplace(ArrowBehaviour bev, ArrowTypes type)
    {
        ArrowBehaviour b;
        if (_prefabs.TryGetValue(type, out b))
        {
            _prefabs.Remove(type);
            _prefabs.Add(type, bev);
        }
        else
        {
            _prefabs.Add(type, bev);
        }
    }

    public ArrowBehaviour GetObjectRequired(ArrowTypes type)
    {
        Queue<ArrowBehaviour> pool = _pool[type];
        if (pool == null)
            return null;

        if (pool.Count <= 0)
            return null;

        ArrowBehaviour arr = pool.Dequeue();
        arr.PoolReference = pool;
        return arr;
    }

    public ArrowBehaviour CreateItem(ArrowTypes type, int amount)
    {
        throw new System.Exception("Not yet Implemented");
    }

    
}
