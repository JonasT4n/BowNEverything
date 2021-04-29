using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

namespace BNEGame
{
    using BNENetwork = Network;

#if UNITY_EDITOR
    namespace Editor
    {
        using UnityEditor;

        [CustomEditor(typeof(GameManager))]
        public class GameManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
        }
    }
#endif

    public enum BNEGameMode { None = 0, Tutorial, SinglePlayer, Multiplayer }
    public enum BNEGameState { None = 0, WarmUp, GameProgress, GameEnded }

    public class GameManager : MonoBehaviour
    {
        [Header("Game System Attributes")]
        [SerializeField] private BNEGameMode gameMode = BNEGameMode.None;
        [SerializeField] private PlayerEntity mainPlayerPrefab = null;
        [SerializeField] private EntitySpawnerManager entitySpawner = null;
        [SerializeField] private ItemSpawnerManager itemSpawner = null;
        [SerializeField] private UIManager uiManager = null;

        [Space, Header("Other Additional Attributes")]
        [SerializeField] private BNENetwork.ServerNonAuthParser serverParser;

        private PlayerEntity localMainPlayer;
        private TutorialScript tutorialScript;
        private BNENetwork.NetworkGameRoomManager multiplayerManager;

        #region Properties
        public BNENetwork.ServerNonAuthParser ServerParser => serverParser;
        public BNEGameState CurrentState { set; get; } = BNEGameState.None;
        public BNEGameMode Mode => gameMode;
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Subscribe events
            UITimer.OnTimerEnd += HandleTimerEnded;
            EventHandler.OnEntityDeathEvent += HandleEntityDeath;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Check if the entity spawner is empty  
            if (entitySpawner == null)
                entitySpawner = FindObjectOfType<EntitySpawnerManager>();

            // Set Up Game by Mode
            SetUpGame();
        }

        private void OnDestroy()
        {
            // Subscribe events
            UITimer.OnTimerEnd -= HandleTimerEnded;
            EventHandler.OnEntityDeathEvent -= HandleEntityDeath;
        }
        #endregion

        #region Event Methods
        private void HandleTimerEnded()
        {
            switch (CurrentState)
            {
                case BNEGameState.WarmUp:
                    EventHandler.CallEvent(new GameStartedEventArgs(CurrentState, BNEGameState.GameProgress));
                    CurrentState = BNEGameState.GameProgress;
                    break;

                case BNEGameState.GameProgress:
                    EventHandler.CallEvent(new GameEndedEventArgs());
                    CurrentState = BNEGameState.GameEnded;
                    break;
            }
        }

        private void HandleEntityDeath(EntityDeathEventArgs args)
        {
            switch (Mode)
            {
                case BNEGameMode.Tutorial:
                    if (args.EntityVictim is PlayerEntity)
                        PlayerRespawn((PlayerEntity)args.EntityVictim, 0);
                    break;

                case BNEGameMode.Multiplayer:
                    if (args.EntityVictim is PlayerEntity)
                        PlayerRespawn((PlayerEntity)args.EntityVictim, 5f);
                    break;
            }
        }
        #endregion

        public void SetUpGame()
        {
            switch (Mode)
            {
                case BNEGameMode.Tutorial:
                    localMainPlayer = Instantiate(mainPlayerPrefab);
                    PlayerRespawn(localMainPlayer, 0);
                    tutorialScript = FindObjectOfType<TutorialScript>();
                    break;

                case BNEGameMode.SinglePlayer:
                    CurrentState = BNEGameState.GameProgress;
                    localMainPlayer = Instantiate(mainPlayerPrefab);
                    PlayerRespawn(localMainPlayer, 0);
                    break;

                case BNEGameMode.Multiplayer:
                    CurrentState = BNEGameState.WarmUp;
                    multiplayerManager = FindObjectOfType<BNENetwork.NetworkGameRoomManager>();
                    break;
            }

            // Set up on ui manager
            uiManager?.SetUpUI(Mode);
        }

        private void PlayerRespawn(PlayerEntity player, float timeDelay)
        {
            if (timeDelay > 0f)
                StartCoroutine(PlayerRespawnDelay(player, 0));
            else
                entitySpawner.RespawnPlayer(player);
        }

        private IEnumerator PlayerRespawnDelay(PlayerEntity player, float timeDelay)
        {
            float t = timeDelay;
            while (t > 0)
            {
                yield return null;
                t -= Time.deltaTime;
            }

            entitySpawner.RespawnPlayer(player);
        }
    }

}
