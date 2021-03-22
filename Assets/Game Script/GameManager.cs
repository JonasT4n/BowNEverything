using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum GameModeState { None = 0, Tutorial, SinglePlayer, MultiPlayer }

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    [Header("Game System Attributes")]
    [SerializeField] private float _defaultRespawnTime = 3f;
    [SerializeField] private GameModeState _gameMode = GameModeState.None;
    [SerializeField] private PlayerEntity _mainPlayerPrefab = null;
    [SerializeField] private EntitySpawner _spawners = null;

    private PlayerEntity _localMainPlayer;

    public GameModeState CurrentGameMode => _gameMode;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        if (_instance)
        {
            Debug.Log($"Deleted multiple object of singleton behaviour: {name}");
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        if (_spawners == null)
            _spawners = FindObjectOfType<EntitySpawner>();

        SetUpGame(CurrentGameMode);

        // Subscribe events
        EventHandler.OnEntityDeathEvent += MPDeathEvent;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void OnDestroy()
    {
        _instance = null;

        // Subscribe events
        EventHandler.OnEntityDeathEvent -= MPDeathEvent;
    }
    #endregion

    #region Event Methods
    private void MPDeathEvent(EntityDeathEventArgs args)
    {
        switch (CurrentGameMode)
        {
            case GameModeState.Tutorial:
                if (args.EntityVictim is PlayerEntity)
                    StartCoroutine(PlayerRespawnDelay((PlayerEntity)args.EntityVictim, 0));
                break;

            case GameModeState.SinglePlayer:
                break;

            case GameModeState.MultiPlayer:
                if (args.EntityVictim is PlayerEntity)
                    StartCoroutine(PlayerRespawnDelay((PlayerEntity)args.EntityVictim, _defaultRespawnTime));
                break;
        }  
    }
    #endregion

    public void SetUpGame(GameModeState gameMode)
    {
        _localMainPlayer = Instantiate(_mainPlayerPrefab);
        _spawners.RespawnPlayer(_localMainPlayer);

        switch (gameMode)
        {
            case GameModeState.Tutorial:
                break;

            case GameModeState.SinglePlayer:
                break;
        }
    }

    private IEnumerator PlayerRespawnDelay(PlayerEntity player, float timeDelay)
    {
        float t = timeDelay;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
        }

        _spawners.RespawnPlayer(player);
    }
}
