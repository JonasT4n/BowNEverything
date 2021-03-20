using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public enum GameModeState { None = 0, Tutorial, SinglePlayer, MultiPlayer }

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;

    [Header("Game System Attributes")]
    [SerializeField] private GameModeState _gameMode = GameModeState.None;
    [SerializeField] private PlayerEntity _mainPlayerPrefab = null;
    [SerializeField] private EntitySpawner _spawners = null;

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
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    #endregion

    public void SetUpGame(GameModeState gameMode)
    {
        switch (gameMode)
        {
            case GameModeState.Tutorial:
                _spawners.RespawnPlayer(Instantiate(_mainPlayerPrefab));
                break;
        }
    }
}
