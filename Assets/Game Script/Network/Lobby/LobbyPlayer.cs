using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace BNEGame.Network
{
    public class LobbyPlayer : NetworkBehaviour, IHaveName
    {
        [SyncVar(hook = nameof(HandlePlayerNameChange))]
        [SerializeField] private string playerName = "";
        [SyncVar(hook = nameof(HandleLeaderChange))]
        private bool isLeader = false;
        [SerializeField] private Text roomPlayerPref = null;

        private Text tempDisplayName;

        private ChatBoxMessager chatBox;
        private MainLobby lobbyMain;
        private NetworkGameRoomManager networkGameManager;

        #region Properties
        public bool IsLeader 
        {
            get => isLeader; 
            set => isLeader = value; 
        }
        public string DisplayName 
        {
            get => playerName;
            set
            {
                playerName = value;
                UpdateUIDisplay();
            }
        }
        private MainLobby LobbyMainMenu
        {
            get
            {
                if (lobbyMain != null) return lobbyMain;
                return lobbyMain = FindObjectOfType<MainLobby>();
            }
        }
        private ChatBoxMessager ChatBox
        {
            get
            {
                if (chatBox != null) return chatBox;
                return chatBox = FindObjectOfType<ChatBoxMessager>();
            }
        }
        public NetworkGameRoomManager RoomLobbyManager
        {
            get
            {
                if (networkGameManager != null) return networkGameManager;
                return networkGameManager = NetworkManager.singleton as NetworkGameRoomManager;
            }
        }
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Create display name on player content list
            tempDisplayName = Instantiate(roomPlayerPref, FindObjectOfType<MainLobby>().PlayerContentPlaceholder);
        }

        public override void OnStartClient()
        {
            RoomLobbyManager.PlayersInRoom.Add(this);

            // Get into room state
            if (isLocalPlayer)
                LobbyMainMenu.SetMultiplayerUIState(MultiplayerUIState.Room);

            UpdateUIDisplay();
        }

        public override void OnStopClient()
        {
            RoomLobbyManager.PlayersInRoom.Remove(this);

            // Go back to lobby state
            if (isLocalPlayer)
                LobbyMainMenu.SetMultiplayerUIState(MultiplayerUIState.Lobby);

            UpdateUIDisplay();
        }

        private void Update()
        {
            if (!isLocalPlayer)
                return;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (ChatBox.IsChatBoxOpen)
                {
                    ChatBox.SendChatMessage();
                    ChatBox.OpenChatBox(false);
                }
                else
                    ChatBox.OpenChatBox(true);
            }
        }

        private void OnDestroy()
        {
            // Destroy ui display
            if (tempDisplayName != null)
            {
                Destroy(tempDisplayName.gameObject);
                tempDisplayName = null;
            }
        }
        #endregion

        private void HandlePlayerNameChange(string oldVal, string newVal) => UpdateUIDisplay();
        private void HandleLeaderChange(bool oldVal, bool newVal) => UpdateUIDisplay();

        #region Network Commands
        [Command]
        public void CmdServerStartGame()
        {
            if (RoomLobbyManager.PlayersInRoom.Count < LobbyMainMenu.MinimumPlayer)
            {
                UpdateUIDisplay();
                return;
            }

            RpcMapUniform(networkGameManager.RandomChooseMapName());
        }

        [ClientRpc]
        private void RpcMapUniform(string mapName)
        {
            LobbyMainMenu.GameSceneManager.LoadScene(mapName);
        }
        #endregion

        public void UpdateUIDisplay()
        {
            string namePref = IsLeader ? $"{playerName} (Host)" : playerName;
            tempDisplayName.text = gameObject.name = namePref;

            if (!hasAuthority)
            {
                foreach (LobbyPlayer player in RoomLobbyManager.PlayersInRoom)
                {
                    if (player.hasAuthority)
                    {
                        player.UpdateUIDisplay();
                        break;
                    }
                }
                return;
            }

            MainLobby roomLobby = FindObjectOfType<MainLobby>();
            if (roomLobby != null)
            {
                roomLobby.UpdateRoomUI();

                if (RoomLobbyManager.PlayersInRoom.Count >= roomLobby.MinimumPlayer)
                    roomLobby.ActivateStartButton = IsLeader;
                else
                    roomLobby.ActivateStartButton = false;
            }
        }
    }

}
