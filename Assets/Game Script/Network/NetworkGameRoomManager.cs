using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using kcp2k;

namespace BNEGame.Network
{
    public enum DisconnectedReason { RequestTimeOut, ServerStopped, LeaveGame }

    [Serializable]
    struct MapSceneNames
    {
        [SerializeField] private string mapName;
        [Scene, SerializeField] private string sceneMapName;

        public string MapName => mapName;
        public string MapScene => sceneMapName;
    }

    public class NetworkGameRoomManager : NetworkManager
    {
        public struct PlayerCreationMsg : NetworkMessage
        {
            public string playerName;
        }

        public static event Action OnClientStarted;
        public static event Action OnClientRoomConnected;
        public static event Action OnClientRoomDisconnected;
        //public static event Action<NetworkConnection> OnPlayerBeforeDisconnected;
        public static event Action<NetworkConnection, string> OnPlayerEnterRoom;
        public static event Action<NetworkConnection, string> OnPlayerLeaveRoom;

        public DisconnectedReason ReasonDisconnect { set; get; } = DisconnectedReason.RequestTimeOut;

        [Space, Header("Additional Information Requirement")]
        [Scene, SerializeField] private string mainLobbyScene = "";
        [SerializeField] private List<MapSceneNames> maps = new List<MapSceneNames>();
        [SerializeField] private List<string> availableAddresses = new List<string>();
        [SerializeField] private LobbyPlayer playerInLobbyPrefab = null;
        [SerializeField] private NetworkInGamePlayer playerInGamePrefab = null;

        private GameManager gameManager;
        private List<LobbyPlayer> playersInRoom = new List<LobbyPlayer>();
        private List<NetworkInGamePlayer> playersInGame = new List<NetworkInGamePlayer>();

        #region Properties
        public string LobbySceneName
        {
            get
            {
                string[] s = mainLobbyScene.Split('/');
                string lobbySceneName = s[s.Length - 1].Split('.')[0];
                return lobbySceneName;
            }
        }
        public GameManager GameMngr
        {
            get
            {
                if (gameManager != null) return gameManager;
                return gameManager = FindObjectOfType<GameManager>();
            }
        }
        public List<LobbyPlayer> PlayersInRoom => playersInRoom;
        public List<NetworkInGamePlayer> PlayersInGame => playersInGame;
        #endregion

        #region Unity BuiltIn Methods
        public override void OnStartServer()
        {
            spawnPrefabs.Clear();
            spawnPrefabs.AddRange(Resources.LoadAll<GameObject>("Spawnable Objects"));

            // Register player creation
            NetworkServer.RegisterHandler<PlayerCreationMsg>(OnPlayerCreate);
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            if (numPlayers >= maxConnections)
            {
                conn.Disconnect();
                return;
            }

            if (SceneManager.GetActiveScene().name != LobbySceneName)
            {
                #if !UNITY_EDITOR
                conn.Disconnect();
                #endif

                return;
            }
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            //// Handle stuff before disconnect
            //OnPlayerBeforeDisconnected?.Invoke(conn);

            if (conn.identity != null)
            {
                LobbyPlayer player = conn.identity.GetComponent<LobbyPlayer>();
                if (player != null)
                {
                    // Remove player from room
                    playersInRoom.Remove(player);

                    // Change leader if there's player in room when the current leader is leaving the room
                    if (playersInRoom.Count > 0 && player.IsLeader)
                        playersInRoom[0].IsLeader = true;

                    // Check disconnection type
                    switch (ReasonDisconnect)
                    {
                        case DisconnectedReason.LeaveGame:
                            OnPlayerLeaveRoom?.Invoke(conn, player.DisplayName);
                            break;
                    }
                }
            }

            base.OnServerDisconnect(conn);
        }

        public override void OnServerChangeScene(string newSceneName)
        {
            if (SceneManager.GetActiveScene().name == LobbySceneName)
            {
                for (int i = playersInRoom.Count - 1; i >= 0; i--)
                {
                    LobbyPlayer lobbyPlayer = playersInRoom[i];
                    NetworkInGamePlayer localPlayerInGame = Instantiate(playerInGamePrefab);
                    localPlayerInGame.DisplayName = lobbyPlayer.DisplayName;

                    NetworkConnection conn = lobbyPlayer.connectionToClient;
                    NetworkServer.Destroy(lobbyPlayer.gameObject);
                    NetworkServer.ReplacePlayerForConnection(conn, localPlayerInGame.gameObject);
                }
            }

            base.OnServerChangeScene(newSceneName);
        }

        public override void OnStartClient()
        {
            spawnPrefabs.Clear();
            GameObject[] spawnablePref = Resources.LoadAll<GameObject>("Spawnable Objects");

            ClientScene.ClearSpawners();

            foreach (GameObject prefab in spawnablePref)
            {
                ClientScene.RegisterPrefab(prefab);
            }

            OnClientStarted?.Invoke();
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            // Adding a new player from client
            conn.Send(new PlayerCreationMsg { playerName = MainLobby.Username });

            // Send player creation message
            //conn.Send(new PlayerCreationMsg { playerName = MainLobby.Username });

            // Invoke client connect event
            OnClientRoomConnected?.Invoke();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);

            // Invoke client disconnect event
            OnClientRoomDisconnected?.Invoke();
        }

        //public override void OnServerAddPlayer(NetworkConnection conn)
        //{
        //    if (SceneManager.GetActiveScene().name == LobbySceneName)
        //    {
        //        LobbyPlayer localPlayer = Instantiate(playerInLobbyPrefab);
        //        localPlayer.IsLeader = playersInRoom.Count == 0;

        //        NetworkServer.AddPlayerForConnection(conn, localPlayer.gameObject);

        //        // After connect to server, Player is officially entered the room
        //        OnPlayerEnterRoom?.Invoke(conn);
        //    }
        //}

        private void OnPlayerCreate(NetworkConnection conn, PlayerCreationMsg msg)
        {
            GameObject player;
            if (SceneManager.GetActiveScene().name == LobbySceneName)
            {
                LobbyPlayer localPlayer = Instantiate(playerInLobbyPrefab);
                localPlayer.DisplayName = msg.playerName;
                localPlayer.IsLeader = playersInRoom.Count == 0;
                player = localPlayer.gameObject;
            }
            else
            {
                NetworkInGamePlayer localPlayer = Instantiate(playerInGamePrefab);
                localPlayer.DisplayName = msg.playerName;
                player = localPlayer.gameObject;
            }

            // Add connection to copied player prefab
            NetworkServer.AddPlayerForConnection(conn, player);

            // After connect to server, Player is officially entered the room
            OnPlayerEnterRoom?.Invoke(conn, player.name);
        }

        public override void OnStopServer()
        {
            playersInRoom.Clear();

            MainLobby lobby = FindObjectOfType<MainLobby>();
            if (lobby != null)
                lobby.ErrorMessage = "Server has been stopped";
        }
#endregion

        public bool TryConnectAvailableAddress()
        {
            foreach (string address in availableAddresses)
            {
                transport.ClientConnect(address);

                if (transport.ClientConnected())
                    return true;
            }

            return false;
        }

        public string RandomChooseMapName()
        {
            int r = UnityEngine.Random.Range(0, maps.Count);
            return maps[r].MapScene;
        }
    }
}

