using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class ItemSpawner : MonoBehaviour, ISpawnerPost
{
    [SerializeField] private FloatingItemBehaviour _prefab = null;
    [SerializeField] private Transform[] _spawnPoints = null;
    [SerializeField] private Vector2 _offsetPos = Vector2.zero;

    [Space, Header("Spawner Attributes")]
    [SerializeField] private float _secondsInterval = 3f;

    private Dictionary<Transform, FloatingItemBehaviour> _createdFloatingItems = new Dictionary<Transform, FloatingItemBehaviour>();

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private float _secIntHandler;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool _isPauseInSinglePlayer = false;

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    private void Start()
    {
        // Starting values
        _secIntHandler = _secondsInterval;

        // Init floating items
        foreach (Transform point in _spawnPoints)
        {
            FloatingItemBehaviour floatingItem = Instantiate(_prefab, transform);
            floatingItem.transform.position = point.position + new Vector3(_offsetPos.x, _offsetPos.y);
            floatingItem.gameObject.SetActive(false);
            _createdFloatingItems.Add(point, floatingItem);
        }

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
        FloatingItemBehaviour f = RandomPointPicker();
        if (f != null)
        {
            f.ItemInfo = RandomInformation(f);
            if (f.ItemInfo == null)
                return;

            f.gameObject.SetActive(true);
        }
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

            if (!_createdFloatingItems[pickedPoint].gameObject.activeSelf)
                return _createdFloatingItems[pickedPoint];

            alreadySpawnedCount++;
        }
        return null;
    }

    private IElementInfo RandomInformation(FloatingItemBehaviour f)
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

        f.RenderSprite = arrowElement.ItemSprite;
        return arrowElement;
    }
}
