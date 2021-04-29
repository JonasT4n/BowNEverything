using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace BNEGame
{
    public class UIManager : MonoBehaviour
    {
        private GameManager gameManager = null;

        [Header("Menu & States")]
        [SerializeField] private RectTransform _gamePanel = null;
        [SerializeField] private RectTransform _pauseMenu = null;
        [SerializeField] private RectTransform _gameOverMenu = null;

        [Space, Header("UI for Main Player Attributes")]
        [SerializeField] private Image _mpCurrentUse = null;
        [SerializeField] private Text _mpName = null;
        [SerializeField] private Text _mpKillCount = null;

        [Space, Header("Other UI")]
        [SerializeField] private UITimer timer = null;

        private InputHandler _inputHandler;
        private Sprite _defaultArrowTypeNoneSprite;

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe events
            EventHandler.OnGameStartedEvent += HandleGameStarting;
            EventHandler.OnGamePauseEvent += HandleGamePaused;
            EventHandler.OnPlayerChangeArrowEvent += HandleMainPlayerChangeArrow;
            EventHandler.OnEntityDeathEvent += HandleEntityDeath;
            EventHandler.OnGameEndedEvent += HandleGameEnding;
        }

        private void Start()
        {
            _inputHandler = FindObjectOfType<InputHandler>();
            gameManager = FindObjectOfType<GameManager>();

            _defaultArrowTypeNoneSprite = _mpCurrentUse.sprite;
        }

        private void FixedUpdate()
        {
            // Handle pause button pressed
            if (gameManager.Mode != BNEGameMode.Multiplayer)
            {
                if (_inputHandler.LocalInputData.PausePressed)
                    PauseGame(!_pauseMenu.gameObject.activeSelf);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe events
            EventHandler.OnGameStartedEvent -= HandleGameStarting;
            EventHandler.OnGamePauseEvent -= HandleGamePaused;
            EventHandler.OnPlayerChangeArrowEvent -= HandleMainPlayerChangeArrow;
            EventHandler.OnEntityDeathEvent -= HandleEntityDeath;
            EventHandler.OnGameEndedEvent -= HandleGameEnding;
        }
        #endregion

        #region Event Methods
        private void HandleGameStarting(GameStartedEventArgs args)
        {
            if (args.PrevState == BNEGameState.WarmUp)
            {
                timer.ResetTimer();
                timer.ActivateTimer(true);
            }
        }

        private void HandleGamePaused(PauseGamePressEventArgs args)
        {
            _pauseMenu.gameObject.SetActive(args.IsPause);
            _gamePanel.gameObject.SetActive(!args.IsPause);
        }

        private void HandleMainPlayerChangeArrow(PlayerChangeArrowEventArgs args)
        {
            if (args.Player.tag != "Player")
                return;

            ArrowQuiverElement e = ObjectManager._instance.GetArrowElement(args.ChangeTo);
            _mpCurrentUse.sprite = e != null ? e.ItemSprite : _defaultArrowTypeNoneSprite;
            _mpName.text = e != null ? e.Name : "None";
        }

        private void HandleEntityDeath(EntityDeathEventArgs args)
        {
            if (args.WhoKill != null)
            {
                if (args.WhoKill.tag == "Player" && args.WhoKill is PlayerEntity)
                {
                    PlayerEntity playerKillEntity = (PlayerEntity)args.WhoKill;
                    _mpKillCount.text = $"Enemy Killed: {playerKillEntity.EnemyKillCount}";
                }

                if (args.EntityVictim.tag == "Player" && args.EntityVictim is PlayerEntity && gameManager.Mode == BNEGameMode.SinglePlayer)
                    EventHandler.CallEvent(new GameEndedEventArgs());

                args.EntityVictim.gameObject.SetActive(false);
            }
        }

        private void HandleGameEnding(GameEndedEventArgs args)
        {
            switch (gameManager.Mode)
            {
                case BNEGameMode.Tutorial:
                    break;

                case BNEGameMode.SinglePlayer:
                    _gameOverMenu.gameObject.SetActive(true);
                    break;

                case BNEGameMode.Multiplayer:
                    break;
            }
        }
        #endregion

        public void SetUpUI(BNEGameMode mode)
        {
            switch (mode)
            {
                case BNEGameMode.Tutorial:
                    break;

                case BNEGameMode.SinglePlayer:
                    break;

                case BNEGameMode.Multiplayer:
                    timer.StartingDuration = 15f;
                    timer.ActivateTimer(true);
                    break;
            }
        }

        public void PauseGame(bool pause)
        {
            if (_pauseMenu.gameObject.activeSelf == pause)
                return;

            PauseGamePressEventArgs arg = new PauseGamePressEventArgs(pause);
            EventHandler.CallEvent(arg);
        }

        public RectTransform GetDefaultGamePanel()
        {
            return _gamePanel;
        }
    }
}