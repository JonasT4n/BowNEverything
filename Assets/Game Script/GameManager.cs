using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameMode { Tutorial, SinglePlayer, MultiPlayer }

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    private static GameMode _gameModeSetUp = GameMode.SinglePlayer;

    [Header("Game System Attributes")]
    [SerializeField] private GameMode _gameMode = GameMode.SinglePlayer;

    public static GameMode CurrentGameMode => _gameModeSetUp;

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
        SetUpGame(_gameMode);
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
    #endregion

    public static void SetUpGame(GameMode g)
    {
        _gameModeSetUp = g;
    }
}
