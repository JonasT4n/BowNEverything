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
    private const int MAX_EACH_POOL = 8;

    public EnemyFactory()
    {

    }

    public void AddOrReplace(EnemyEntity bev, EnemyType type)
    {

    }

    public EnemyEntity GetObjectRequired(EnemyType type)
    {
        throw new System.NotImplementedException();
    }

    public EnemyEntity CreateItem(EnemyType type, int amount)
    {
        throw new System.NotImplementedException();
    }
}
