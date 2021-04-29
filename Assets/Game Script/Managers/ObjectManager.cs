using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace BNEGame
{
    public class ObjectManager : MonoBehaviour
    {
        [System.Serializable]
        private struct ArrowRarityPair
        {
            public string name;
            public GameRarity rarity;
            public ArrowTypes arrowType;
        }

        public static ObjectManager _instance;

        private ArrowFactory _arrowFactory;
        private EnemyFactory _enemyFactory;

        private static Dictionary<GameRarity, List<ArrowTypes>> _rarities = new Dictionary<GameRarity, List<ArrowTypes>>();
        private Dictionary<ArrowTypes, ArrowQuiverElement> _kindsOfArrow = new Dictionary<ArrowTypes, ArrowQuiverElement>();
        private Dictionary<EnemyType, EnemyStructureElement> _kindsOfEnemy = new Dictionary<EnemyType, EnemyStructureElement>();

        [InfoBox("Quiver element are kinds of existing arrows, you can add new arrow in this section.")]
        [SerializeField] private ArrowQuiverElement[] _arrows = null;
        [InfoBox("Kinds of enemies, add more new kind of enemy here in this section.")]
        [SerializeField] private EnemyStructureElement[] _enemies = null;
        [InfoBox("Define every arrow rarities, each type can only be in one of the existing rarities.")]
        [SerializeField] private ArrowRarityPair[] _rarityPairs = null;

        #region Properties
        public ArrowFactory ArrowMaker => _arrowFactory;
        public EnemyFactory EnemyMaker => _enemyFactory;
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Make class singleton
            if (_instance)
            {
                Debug.Log($"Deleted multiple object of singleton behaviour: {name}");
                Destroy(this);
                return;
            }
            else
            {
                _instance = this;
            }

            // Init rarities in game
            foreach (ArrowRarityPair pair in _rarityPairs)
            {
                List<ArrowTypes> t;
                if (_rarities.TryGetValue(pair.rarity, out t))
                {
                    if (!t.Contains(pair.arrowType))
                        t.Add(pair.arrowType);
                }
                else
                {
                    _rarities.Add(pair.rarity, new List<ArrowTypes>());
                    _rarities[pair.rarity].Add(pair.arrowType);
                }
            }

            // Init objects to be pulled, in this case every kind of arrows
            Dictionary<ArrowTypes, ArrowBehaviour> arrows = new Dictionary<ArrowTypes, ArrowBehaviour>();
            foreach (ArrowQuiverElement e in _arrows)
            {
                if (arrows.ContainsKey(e.Type))
                    continue;

                arrows.Add(e.Type, e.ItemPrefab);
                _kindsOfArrow.Add(e.Type, e);
            }
            _arrowFactory = new ArrowFactory(arrows, transform);

            Dictionary<EnemyType, EnemyEntity> enemies = new Dictionary<EnemyType, EnemyEntity>();
            foreach (EnemyStructureElement e in _enemies)
            {
                if (enemies.ContainsKey(e.Type))
                    continue;

                enemies.Add(e.Type, e.EnemyPrefab);
                _kindsOfEnemy.Add(e.Type, e);
            }
            _enemyFactory = new EnemyFactory(enemies);
        }

        private void OnDestroy()
        {
            _instance = null;
        }
        #endregion

        public static List<ArrowTypes> GetTypesByRarity(GameRarity rarity)
        {
            List<ArrowTypes> t;
            if (_rarities.TryGetValue(rarity, out t))
                return t;
            return null;
        }

        public ArrowQuiverElement GetArrowElement(ArrowTypes type)
        {
            ArrowQuiverElement e;
            if (_kindsOfArrow.TryGetValue(type, out e))
                return e;
            return null;
        }

        public EnemyStructureElement GetEnemyElement(EnemyType type)
        {
            EnemyStructureElement e;
            if (_kindsOfEnemy.TryGetValue(type, out e))
                return e;
            return null;
        }
    }
}