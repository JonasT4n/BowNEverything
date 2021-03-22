using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Dummy = 0,
    Grunts = 1,
    Ranged = 2,
    Caster = 3
}

public class EnemyFactory : IFactoryWithPool<EnemyEntity, EnemyType>
{
    private const int MAX_EACH_POOL = 25;

    private Transform _poolContainer;
    private Dictionary<EnemyType, EnemyEntity> _prefabs;
    private Dictionary<EnemyType, Queue<EnemyEntity>> _pool = new Dictionary<EnemyType, Queue<EnemyEntity>>();

    public EnemyFactory(Dictionary<EnemyType, EnemyEntity> prefabs, Transform container = null)
    {
        _prefabs = prefabs;
        _poolContainer = container;

        foreach (EnemyType e in _prefabs.Keys)
        {
            EnemyEntity enemyPref = _prefabs[e];
            _pool.Add(e, new Queue<EnemyEntity>());

            for (int i = 0; i < MAX_EACH_POOL; i++)
            {
                EnemyEntity dupe = Object.Instantiate(enemyPref);
                dupe.InformationUI.MaxHealthValue = dupe.MaxHealth;
                dupe.gameObject.SetActive(false);

                if (_poolContainer != null)
                    dupe.transform.parent = _poolContainer;
                _pool[e].Enqueue(dupe);
            }
        }
    }

    public void AddOrReplace(EnemyEntity bev, EnemyType type)
    {
        EnemyEntity b;
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

    public EnemyEntity GetObjectRequired(EnemyType type)
    {
        Queue<EnemyEntity> pool;
        if (_pool.TryGetValue(type, out pool))
        {
            if (pool.Count <= 0)
                return null;

            EnemyEntity arr = pool.Dequeue();
            arr.PoolReference = pool;
            return arr;
        }
        return null;
    }

    public EnemyEntity CreateItem(EnemyType type, int amount)
    {
        throw new System.NotImplementedException();
    }
}
