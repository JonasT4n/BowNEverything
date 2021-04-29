using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

namespace BNEGame
{
    public class EntitySpawnerManager : MonoBehaviour, ISpawnerPost
    {
        private const float SAFE_DISTANCE = 5f;

        [Header("Spawner Requirements")]
        [SerializeField] private GameManager gameManager = null;
        [SerializeField] private Transform[] _spawnPoints = null;
        [SerializeField] private Transform[] _enemySpawnPoints = null;
        [SerializeField] private Vector2 _offsetPos = Vector2.zero;

        [Space, Header("Spawner Attributes")]
        [SerializeField] private float _secondsInterval = 3f;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _secIntHandler;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isPauseInSinglePlayer = false;

        #region Unity BuiltIn Methods
        // Start is called before the first frame update
        private void Start()
        {
            // Starting values
            _secIntHandler = _secondsInterval;

            // Subscribe event
            EventHandler.OnGamePauseEvent += SpawnerPause;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (!_isPauseInSinglePlayer)
            {
                if (_secIntHandler > 0f)
                {
                    _secIntHandler -= Time.deltaTime;
                }
                else
                {
                    _secIntHandler = _secondsInterval;
                    if (gameManager.Mode != BNEGameMode.Tutorial && gameManager.Mode != BNEGameMode.None)
                        Spawn();
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe event
            EventHandler.OnGamePauseEvent -= SpawnerPause;
        }
        #endregion

        private void SpawnerPause(PauseGamePressEventArgs args)
        {
            _isPauseInSinglePlayer = args.IsPause;
        }

        public void Spawn()
        {
            Transform point = null;
            EnemyType enemyTypeSpawned = EnemyType.Grunts;

            switch (gameManager.Mode)
            {
                case BNEGameMode.Tutorial:
                    point = _enemySpawnPoints[0];
                    enemyTypeSpawned = EnemyType.Dummy;
                    break;

                case BNEGameMode.SinglePlayer:
                    point = RandomPointPicker(_enemySpawnPoints);
                    enemyTypeSpawned = RandomEnemySpawnType();
                    break;
            }

            if (point != null)
            {
                EnemyEntity ent = ObjectManager._instance.EnemyMaker.GetObjectRequired(enemyTypeSpawned);
                if (ent == null)
                    return;

                ent.transform.position = point.transform.position;
                if (!ent.gameObject.activeSelf)
                    ent.gameObject.SetActive(true);
            }
        }

        public void RespawnPlayer(PlayerEntity player)
        {
            Transform safePointSpawn = null;

            switch (gameManager.Mode)
            {
                case BNEGameMode.Tutorial:
                    safePointSpawn = _spawnPoints[0];
                    break;

                case BNEGameMode.SinglePlayer:
                    safePointSpawn = RandomPointPicker(_spawnPoints);
                    break;
            }

            // Call player respawn event
            PlayerRespawnEventArgs arg = new PlayerRespawnEventArgs(player);
            EventHandler.CallEvent(arg);
            if (!arg.IsCancelled)
            {
                if (safePointSpawn != null)
                    player.transform.position = safePointSpawn.position + new Vector3(_offsetPos.x, _offsetPos.y, 0f);
                if (!player.gameObject.activeSelf)
                    player.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Pick spawn point randomly.
        /// </summary>
        /// <returns>Safe area transform point</returns>
        private Transform RandomPointPicker(Transform[] points)
        {
            List<Transform> listPoints = new List<Transform>(points);
            while (listPoints.Count > 1)
            {
                int pickedIndex = Random.Range(0, listPoints.Count);
                Transform pickedPoint = listPoints[pickedIndex];
                listPoints.RemoveAt(pickedIndex);

                if (IsAreaSafe(pickedPoint))
                    return pickedPoint;
            }
            return listPoints[0];
        }

        private EnemyType RandomEnemySpawnType()
        {
            // TODO: Use dynamic if all enemy exists
            //(EnemyType)Random.Range(1, (int)System.Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>().Max() + 1);

            EnemyType[] enemies = new EnemyType[2] { EnemyType.Grunts, EnemyType.Caster };
            return enemies[Random.Range(0, enemies.Length)];
        }

        private bool IsAreaSafe(Transform point)
        {
            LivingEntity[] ents = FindObjectsOfType<LivingEntity>();
            foreach (LivingEntity e in ents)
            {
                float distance = Vector3.Distance(e.transform.position, point.position + new Vector3(_offsetPos.x, _offsetPos.y, 0f));
                if (distance <= SAFE_DISTANCE)
                    return false;
            }
            return true;
        }
    }
}