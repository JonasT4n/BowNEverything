using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum SingleGameMode { None = 0, Tutorial, SinglePlayer}

public class GameManager : MonoBehaviour
{
    [Header("Game System Attributes")]
    [SerializeField] private SingleGameMode _gameMode = SingleGameMode.None;
    [SerializeField] private PlayerEntity _mainPlayerPrefab = null;
    [SerializeField] private EntitySpawner _spawners = null;

    private PlayerEntity _localMainPlayer;

    public SingleGameMode Mode => _gameMode;

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    private void Start()
    {
        if (_spawners == null)
            _spawners = FindObjectOfType<EntitySpawner>();

        SetUpGame(Mode);

        // Subscribe events
        EventHandler.OnEntityDeathEvent += MPDeathEvent;
    }

    private void OnDestroy()
    {
        // Subscribe events
        EventHandler.OnEntityDeathEvent -= MPDeathEvent;
    }
    #endregion

    #region Event Methods
    private void MPDeathEvent(EntityDeathEventArgs args)
    {
        switch (Mode)
        {
            case SingleGameMode.Tutorial:
                if (args.EntityVictim is PlayerEntity)
                    StartCoroutine(PlayerRespawnDelay((PlayerEntity)args.EntityVictim, 0));
                break;

            case SingleGameMode.SinglePlayer:
                break;
        }  
    }
    #endregion

    public void SetUpGame(SingleGameMode mode)
    {
        _localMainPlayer = Instantiate(_mainPlayerPrefab);
        _spawners.RespawnPlayer(_localMainPlayer);

        switch (mode)
        {
            case SingleGameMode.Tutorial:
                break;

            case SingleGameMode.SinglePlayer:
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
