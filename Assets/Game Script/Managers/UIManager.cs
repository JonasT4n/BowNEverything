using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager = null;

    [Header("Menu & States")]
    [SerializeField] private RectTransform _gamePanel = null;
    [SerializeField] private RectTransform _pauseMenu = null;
    [SerializeField] private RectTransform _gameOverMenu = null;

    [Header("UI for Main Player Attributes")]
    [SerializeField] private Image _mpCurrentUse = null;
    [SerializeField] private Text _mpName = null;
    [SerializeField] private Text _mpKillCount = null;

    private InputHandler _inputHandler;
    private Sprite _defaultArrowTypeNoneSprite;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        _inputHandler = FindObjectOfType<InputHandler>();
        if (_gameManager == null)
            _gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        _defaultArrowTypeNoneSprite = _mpCurrentUse.sprite;

        // Subscribe events
        EventHandler.OnGamePauseEvent += GamePauseEvent;
        EventHandler.OnPlayerChangeArrowEvent += ChangeArrowEvent;
        EventHandler.OnEntityDeathEvent += WhenEntityDeath;
        EventHandler.OnGameEndedEvent += GameOverEvent;
    }

    private void FixedUpdate()
    {
        // Handle pause button pressed
        if (_inputHandler.LocalInputData.PausePressed)
            PauseGame(!_pauseMenu.gameObject.activeSelf);
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        EventHandler.OnGamePauseEvent -= GamePauseEvent;
        EventHandler.OnPlayerChangeArrowEvent -= ChangeArrowEvent;
        EventHandler.OnEntityDeathEvent -= WhenEntityDeath;
        EventHandler.OnGameEndedEvent -= GameOverEvent;
    }
    #endregion

    #region Event Methods
    private void GamePauseEvent(PauseGamePressEventArgs args)
    {
        _pauseMenu.gameObject.SetActive(args.IsPause);
        _gamePanel.gameObject.SetActive(!args.IsPause);
    }

    private void ChangeArrowEvent(PlayerChangeArrowEventArgs args)
    {
        if (args.Player.tag != "MainPlayer")
            return;

        ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(args.ChangeTo);
        _mpCurrentUse.sprite = e != null ? e.ItemSprite : _defaultArrowTypeNoneSprite;
        _mpName.text = e != null ? e.Name : "Unknown";
    }

    private void WhenEntityDeath(EntityDeathEventArgs args)
    {
        if (args.WhoKill != null)
        {
            if (args.WhoKill.tag == "MainPlayer" && args.WhoKill is PlayerEntity)
            {
                PlayerEntity playerKillEntity = (PlayerEntity)args.WhoKill;
                _mpKillCount.text = $"Enemy Killed: {playerKillEntity.EnemyKillCount}";
            }

            if (args.EntityVictim.tag == "MainPlayer" && args.EntityVictim is PlayerEntity && _gameManager.Mode == SingleGameMode.SinglePlayer)
                EventHandler.CallEvent(new GameEndedEventArgs());

            args.EntityVictim.gameObject.SetActive(false);
        }
    }

    private void GameOverEvent(GameEndedEventArgs args)
    {
        _gameOverMenu.gameObject.SetActive(true);
    }
    #endregion

    public void PauseGame(bool pause)
    {
        if (_pauseMenu.gameObject.activeSelf == pause)
            return;

        PauseGamePressEventArgs arg = new PauseGamePressEventArgs(_gameManager.Mode, pause);
        EventHandler.CallEvent(arg);
    }

    public RectTransform GetDefaultGamePanel()
    {
        return _gamePanel;
    }
}
