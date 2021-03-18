using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance;

    [Header("Menu & States")]
    [SerializeField] private RectTransform _gamePanel = null;
    [SerializeField] private RectTransform _pauseMenu = null;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        // Make class singleton
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

    private void Start()
    {
        // Subscribe events
        EventHandler.OnGamePauseEvent += GamePauseEvent;
    }

    private void Update()
    {
        // Handle pause button pressed
        if (InputHandler.LocalInputData.PausePressed)
            PauseGame(!_pauseMenu.gameObject.activeSelf);
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        EventHandler.OnGamePauseEvent -= GamePauseEvent;
    }
    #endregion

    #region Event Methods
    private void GamePauseEvent(PauseGamePressEventArgs args)
    {
        _pauseMenu.gameObject.SetActive(args.IsPause);
        _gamePanel.gameObject.SetActive(!args.IsPause);
    }
    #endregion

    public void PauseGame(bool pause)
    {
        if (_pauseMenu.gameObject.activeSelf == pause)
            return;

        if (GameManager._instance != null)
        {
            PauseGamePressEventArgs arg = new PauseGamePressEventArgs(GameManager.CurrentGameMode, pause);
            EventHandler.CallEvent(arg);
        }
    }
}
