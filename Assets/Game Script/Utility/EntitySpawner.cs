using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class EntitySpawner : MonoBehaviour, ISpawner
{
    private const float SAFE_DISTANCE = 5f;

    [SerializeField] private Transform[] _spawnPoints = null;
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
    private void Update()
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
        if (args.Mode != GameModeState.MultiPlayer)
            _isPauseInSinglePlayer = args.IsPause;
    }

    public void Spawn()
    {
        
    }

    public void RespawnPlayer(PlayerEntity ent)
    {
        Transform safePointSpawn = RandomPointPicker();
        ent.transform.position = safePointSpawn.position + new Vector3(_offsetPos.x, _offsetPos.y, 0f);

        // TODO: Temporary condition, make it into event
        if (!ent.gameObject.activeSelf)
            ent.gameObject.SetActive(true);
    }

    /// <summary>
    /// Pick spawn point randomly.
    /// </summary>
    /// <returns>Safe area transform point</returns>
    private Transform RandomPointPicker()
    {
        List<Transform> listPoints = new List<Transform>(_spawnPoints);
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
