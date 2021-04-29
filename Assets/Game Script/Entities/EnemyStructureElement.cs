using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace BNEGame
{
    [System.Serializable]
    public class EnemyStructureElement
    {
        [SerializeField] private string _name = "";
        [SerializeField] private EnemyType _type = EnemyType.Dummy;
        [SerializeField] private EnemyEntity _prefab = null;

        public string Name => _name;
        public EnemyType Type => _type;
        public EnemyEntity EnemyPrefab => _prefab;
    }
}