using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BNEGame.Network
{
    public class OnlineGameManager : NetworkManager
    {
        [Header("Online Game System Attributes")]
        [SerializeField] private float _defaultRespawnTime = 3f;
        [SerializeField] private ItemSpawnerInfoParser _itemSpawnerParser = null;
        [SerializeField] private RectTransform _gamePanel = null;
        [SerializeField] private ChatBoxMessager _messager = null;

        private bool _isGameAlreadyStarted = false;

        #region Unity BuiltIn Methods
        public override void OnStartServer()
        {
            base.OnStartServer();

            RegisterHandlers(true);
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);

            NetworkInGamePlayer playerInfo = conn.identity?.GetComponent<NetworkInGamePlayer>();
            if (playerInfo != null)
            {

            }
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {

            base.OnClientDisconnect(conn);
        }

        public override void OnStopServer()
        {

            base.OnStopServer();

            RegisterHandlers(false);
        }

        public override void Start()
        {
            base.Start();

            // Subscribe events
            EventHandler.OnEntityDeathEvent += MPDeathEvent;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe events
            EventHandler.OnEntityDeathEvent -= MPDeathEvent;
        }
        #endregion

        #region Event Methods
        private void MPDeathEvent(EntityDeathEventArgs args)
        {

        }
        #endregion

        private void RegisterHandlers(bool enable)
        {

        }

        private IEnumerator PlayerRespawnDelay(PlayerEntity player, float timeDelay)
        {
            float t = timeDelay;
            while (t > 0)
            {
                yield return null;
                t -= Time.deltaTime;
            }
        }

        public RectTransform GetDefaultGamePanel()
        {
            return _gamePanel;
        }
    }

}
