using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

namespace BNEGame
{
    using NetworkBNE = Network;

    public class ItemSpawnerManager : MonoBehaviour, ISpawnerPost
    {
        [SerializeField] private FloatingItemBehaviour _prefab = null;
        [SerializeField] private Transform[] _spawnPoints = null;
        [SerializeField] private Vector2 _offsetPos = Vector2.zero;

        [Space, Header("Spawner Attributes")]
        [SerializeField] private float secondsInterval = 3f;

        private Dictionary<Transform, FloatingItemBehaviour> floatingItems = new Dictionary<Transform, FloatingItemBehaviour>();

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private int lastIndexSpawned;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float secondsIntervalHandler;
        [BoxGroup("DEBUG"), SerializeField] private bool isRunning = false;

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe event
            EventHandler.OnGameStartedEvent += GameStartedEvent;
            EventHandler.OnGameEndedEvent += GameEndedEvent;
            EventHandler.OnGamePauseEvent += SpawnerPause;
        }

        private void Start()
        {
            // Starting values
            secondsIntervalHandler = secondsInterval;

            // Init floating items
            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                Transform point = _spawnPoints[i];
                FloatingItemBehaviour floatingItem = Instantiate(_prefab, transform);
                floatingItem.transform.position = point.position + new Vector3(_offsetPos.x, _offsetPos.y);
                floatingItem.gameObject.SetActive(false);
                floatingItem.OnCollectionIndex = i;
                floatingItems.Add(point, floatingItem);
            }
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (isRunning)
            {
                if (secondsIntervalHandler > 0f)
                {
                    secondsIntervalHandler -= Time.deltaTime;
                }
                else
                {
                    secondsIntervalHandler = secondsInterval;
                    Spawn();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe event
            EventHandler.OnGameStartedEvent -= GameStartedEvent;
            EventHandler.OnGameEndedEvent -= GameEndedEvent;
            EventHandler.OnGamePauseEvent -= SpawnerPause;
        }
        #endregion

        #region Event Methods
        private void GameStartedEvent(GameStartedEventArgs args)
        {
            SetRunning(true);
        }

        private void GameEndedEvent(GameEndedEventArgs args)
        {
            SetRunning(false);
        }

        private void SpawnerPause(PauseGamePressEventArgs args)
        {
            if (FindObjectOfType<NetworkBNE.OnlineGameManager>() == null)
                SetRunning(!args.IsPause);
        }
        #endregion

        public void Spawn()
        {
            FloatingItemBehaviour f = RandomPointPicker();
            if (f != null)
            {
                f.ItemInfo = RandomInformation();
                if (f.ItemInfo == null)
                    return;

                ItemSpawnedEventArgs arg = new ItemSpawnedEventArgs(lastIndexSpawned, f.ItemInfo);
                EventHandler.CallEvent(arg);

                if (!arg.IsCancelled)
                    f.gameObject.SetActive(true);
            }
        }

        public void SetRunning(bool run)
        {
            isRunning = run;
        }

        public FloatingItemBehaviour GetSpawnerPoint(int spawnerIndex)
        {
            if (spawnerIndex >= _spawnPoints.Length)
                return null;

            Transform trans = _spawnPoints[spawnerIndex];
            if (!floatingItems.ContainsKey(trans))
                return null;

            return floatingItems[trans];
        }

        /// <summary>
        /// Get point which will be spawned.
        /// </summary>
        /// <returns>null if all spawner already spawned</returns>
        private FloatingItemBehaviour RandomPointPicker()
        {
            int alreadySpawnedCount = 0;
            List<Transform> listPoints = new List<Transform>(_spawnPoints);
            while (alreadySpawnedCount < _spawnPoints.Length)
            {
                int pickedIndex = Random.Range(0, listPoints.Count);
                Transform pickedPoint = listPoints[pickedIndex];
                listPoints.RemoveAt(pickedIndex);

                if (!floatingItems[pickedPoint].gameObject.activeSelf)
                {
                    lastIndexSpawned = pickedIndex;
                    return floatingItems[pickedPoint];
                }

                alreadySpawnedCount++;
            }
            return null;
        }

        private ArrowQuiverElement RandomInformation()
        {
            // TODO: Make all rarity exists
            //GameRarity rarity = (GameRarity)Random.Range(0, (int)System.Enum.GetValues(typeof(GameRarity)).Cast<GameRarity>().Max() + 1);

            GameRarity[] rarities = new GameRarity[4] { GameRarity.Trash, GameRarity.Common, GameRarity.Rare, GameRarity.Legendary };
            List<ArrowTypes> t = ObjectManager.GetTypesByRarity(rarities[Random.Range(0, rarities.Length)]);

            if (t == null)
                return null;

            int pickedInd = Random.Range(0, t.Count);
            ArrowQuiverElement arrowElement = ObjectManager._instance.GetArrowElement(t[pickedInd]);
            if (arrowElement == null)
                return null;

            return arrowElement;
        }

        public void ClearSpawner()
        {
            foreach (FloatingItemBehaviour fItem in floatingItems.Values)
            {
                if (fItem.gameObject.activeSelf)
                    fItem.gameObject.SetActive(false);
            }
        }
    }
}