using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using NaughtyAttributes;

namespace BNEGame.Network
{
    public enum MultiplayerUIState { Lobby, SearchQueue, Room, Game }

    public class MainLobby : MonoBehaviour
    {
        [SerializeField] private int minPlayer = 2;
        [SerializeField] private int maxPlayer = 4;
        [SerializeField] private int topPaddingEachContent = 8;
        [SerializeField] private ChatBoxMessager chatBoxMessager = null;
        [SerializeField] private UIGameSceneManager sceneManager = null;
        [SerializeField] private ServerNonAuthParser serverParser = null;

        [Header("UI")]
        [SerializeField] private Button hostingButton = null;
        [SerializeField] private Button joinButton = null;
        [SerializeField] private Button startButton = null;

        [SerializeField] private InputField username = null;
        [SerializeField] private InputField ipAddress = null;

        [SerializeField] private Text roomCapacity = null;
        [SerializeField] private Text errorMessage = null;

        [SerializeField] private RectTransform lobbyPage = null;
        [SerializeField] private RectTransform searchingPage = null;
        [SerializeField] private RectTransform roomPage = null;
        [SerializeField] private RectTransform contentMasterPlayer = null;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private NetworkGameRoomManager networkGameManager = null;

        #region Properties
        public static string IpAddress { get; private set; }
        public static string Username { get; private set; }
        public static MultiplayerUIState UIState { get; private set; } = MultiplayerUIState.Lobby;
        public bool ActivateLobbyInteraction
        {
            set
            {
                hostingButton.interactable = value;
                joinButton.interactable = value;

                username.interactable = value;
                ipAddress.interactable = value;
            }
        }
        public bool ActivateStartButton 
        {
            set
            {
                startButton.interactable = value;
            }
        }
        public int MinimumPlayer => minPlayer;
        public ChatBoxMessager ChatBox => chatBoxMessager;
        public Transform PlayerContentPlaceholder => contentMasterPlayer;
        public NetworkGameRoomManager RoomLobbyManager
        {
            get
            {
                if (networkGameManager != null) return networkGameManager;
                return networkGameManager = NetworkManager.singleton as NetworkGameRoomManager;
            }
        }
        public string ErrorMessage 
        { 
            set
            {
                errorMessage.text = value;
                errorMessage.gameObject.SetActive(true);
            }
        }
        public UIGameSceneManager GameSceneManager => sceneManager;
        #endregion

        #region Unity BuiltIn Methods
        private void OnEnable()
        {
            // Check starting state
            SetMultiplayerUIState(UIState);

            // Subscribe events
            NetworkGameRoomManager.OnClientStarted += HandleClientStarting;
            NetworkGameRoomManager.OnClientRoomConnected += HandleClientConnectToRoom;
            NetworkGameRoomManager.OnClientRoomDisconnected += HandleClientDisconnectedFromRoom;
            NetworkGameRoomManager.OnPlayerEnterRoom += HandlePlayerEnter;
            NetworkGameRoomManager.OnPlayerLeaveRoom += HandlePlayerLeave;
        }

        private void Start()
        {
            // Get all required components
            networkGameManager = FindObjectOfType<NetworkGameRoomManager>();

            // Set initial page
            lobbyPage.gameObject.SetActive(true);
            searchingPage.gameObject.SetActive(false);
            roomPage.gameObject.SetActive(false);

            // Set all initial values to all UI elements
            username.text = Username;
            ipAddress.text = IpAddress;
            startButton.interactable = false;
        }

        private void OnDisable()
        {
            // Unsbscribe events
            NetworkGameRoomManager.OnClientStarted -= HandleClientStarting;
            NetworkGameRoomManager.OnClientRoomConnected -= HandleClientConnectToRoom;
            NetworkGameRoomManager.OnClientRoomDisconnected -= HandleClientDisconnectedFromRoom;
            NetworkGameRoomManager.OnPlayerEnterRoom -= HandlePlayerEnter;
            NetworkGameRoomManager.OnPlayerLeaveRoom -= HandlePlayerLeave;
        }
        #endregion

        #region Event Methods
        private void HandleClientStarting()
        {
            errorMessage.text = "Connecting...";
            errorMessage.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called on client when connected.
        /// </summary>
        private void HandleClientConnectToRoom()
        {
            // Set inside room state
            //SetMultiplayerUIState(MultiplayerUIState.Room);

            ActivateLobbyInteraction = false;
        }

        /// <summary>
        /// Called on client when disconnected.
        /// </summary>
        private void HandleClientDisconnectedFromRoom()
        {
            // Set into lobby state
            //SetMultiplayerUIState(MultiplayerUIState.Lobby);

            username.text = Username;
            ipAddress.text = IpAddress;

            ActivateLobbyInteraction = true;

            switch (RoomLobbyManager.ReasonDisconnect)
            {
                case DisconnectedReason.LeaveGame:
                    errorMessage.text = "You have leave the room";
                    break;

                case DisconnectedReason.RequestTimeOut:
                    errorMessage.text = "Request Timeout";
                    break;

                case DisconnectedReason.ServerStopped:
                    errorMessage.text = "Server was stopped";
                    break;
            }

            errorMessage.gameObject.SetActive(true);
        }

        private void HandlePlayerEnter(NetworkConnection conn, string playerName)
        {
            // Notify player joined the server
            serverParser.CmdSendChatMessage($"{playerName} joined the server!", Color.green, true);

            UpdateRoomUI();
            ActivateLobbyInteraction = false;
        }

        /// <summary>
        /// Called on Server.
        /// </summary>
        /// <param name="conn">Connection to Client with Server</param>
        /// <param name="playerName">Player Name who leave the game</param>
        private void HandlePlayerLeave(NetworkConnection conn, string playerName)
        {
            // Update UI
            UpdateRoomUI();
            ActivateLobbyInteraction = true;
        }
        #endregion

        public void HostingLobby()
        {
            networkGameManager.StartHost();
        }

        public void JoinRoom()
        {
            if (string.IsNullOrEmpty(IpAddress))
            {
                SearchForRoom();
                return;
            }

            networkGameManager.networkAddress = IpAddress;
            networkGameManager.StartClient();

            ActivateLobbyInteraction = false;
        }

        private void SearchForRoom()
        {
            SetMultiplayerUIState(MultiplayerUIState.SearchQueue);

            bool connect = networkGameManager.TryConnectAvailableAddress();

            if (!connect)
                SetMultiplayerUIState(MultiplayerUIState.Lobby);
        }

        public void StartGame()
        {
            LobbyPlayer player = NetworkClient.connection.identity?.GetComponent<LobbyPlayer>();
            if (player.hasAuthority)
            {
                player.CmdServerStartGame();
            }
        }

        public void LeaveRoom()
        {
            // Set disconnect reason
            RoomLobbyManager.ReasonDisconnect = DisconnectedReason.LeaveGame;

            // Identify whether the client is with the server or not
            NetworkIdentity playerId = NetworkClient.connection.identity;
            if (playerId == null ? false : playerId.isServer)
            {
                FindObjectOfType<ChatBoxMessager>()?.SendMessageToChatBox(new MsgLine
                {
                    msg = $"Server has been stopped",
                    color = Color.magenta
                }, true);

                networkGameManager.StopHost();
            }
            else
            {
                // Notify player leave the room
                string playerName = playerId == null ? Username :
                    playerId.GetComponent<LobbyPlayer>() == null ? Username :
                    playerId.GetComponent<LobbyPlayer>().DisplayName;
                serverParser.CmdSendChatMessage($"{playerName} leave the server!", Color.red, true);

                networkGameManager.StopClient();
            }
        }

        public void CorrectingInputIpAddress()
        {
            string[] address = ipAddress.text.Trim().Split('.');
            if (address.Length == 4)
            {
                bool isIpAddress = true;
                for (int i = 0; i < address.Length; i++)
                {
                    string ipBlock = address[i];
                    int ipBlockInt;
                    if (!int.TryParse(ipBlock, out ipBlockInt))
                    {
                        isIpAddress = false;
                        break;
                    }
                    else
                    {
                        ipBlockInt = ipBlockInt > 255 ? 255 : (ipBlockInt < 0) ? 0 : ipBlockInt;
                        address[i] = ipBlockInt.ToString();
                    }
                }

                if (isIpAddress)
                {
                    ipAddress.text = $"{address[0]}.{address[1]}.{address[2]}.{address[3]}";
                    return;
                }
            }
            else if (ipAddress.text.Trim() == "")
            {
                ipAddress.text = string.Empty;
                return;
            }

            ipAddress.text = "localhost";
        }

        public void SetMultiplayerUIState(MultiplayerUIState state)
        {
            UIState = state;
            switch (UIState)
            {
                case MultiplayerUIState.Lobby:
                    lobbyPage.gameObject.SetActive(true);
                    searchingPage.gameObject.SetActive(false);
                    roomPage.gameObject.SetActive(false);
                    break;

                case MultiplayerUIState.Room:
                    lobbyPage.gameObject.SetActive(false);
                    searchingPage.gameObject.SetActive(false);
                    roomPage.gameObject.SetActive(true);
                    break;

                case MultiplayerUIState.SearchQueue:
                    lobbyPage.gameObject.SetActive(false);
                    searchingPage.gameObject.SetActive(true);
                    roomPage.gameObject.SetActive(false);
                    break;
            }
        }

        public void CheckValidation()
        {
            // Regex for ip address
            bool isValid = true;
            string ipAddressPattern = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            Regex reg = new Regex(ipAddressPattern);

            // Check ip validation
            if (ipAddress.text.Trim() == "localhost") { }
            else if (!string.IsNullOrEmpty(reg.Match(ipAddress.text.Trim()).Value)) { }
            else
            {
                errorMessage.text = "Invalid Ip Address";
                isValid = false;
            }

            // Check username validation
            if (username.text.Trim() == "")
            {
                errorMessage.text = "Your username cannot be empty!";
                isValid = false;
            }

            // Check if it is already valid input
            if (!isValid)
            {
                errorMessage.gameObject.SetActive(ipAddress.text.Trim() == "" && username.text.Trim() != "" ? false : true);
                hostingButton.interactable = false;

                // Check only if the ip is empty but username is not empty, special case for matchmaking
                joinButton.interactable = ipAddress.text.Trim() == "" && username.text.Trim() != "" ? true : false;
            }
            else
            {
                // All input are valid
                errorMessage.gameObject.SetActive(false);
                hostingButton.interactable = true;
                joinButton.interactable = true;
            }
        }

        public void SetIpAddress()
        {
            IpAddress = ipAddress.text.Trim();
        }

        public void SetPlayerName()
        {
            Username = username.text.Trim();
        }

        public void UpdateRoomUI()
        {
            // Sync hint on how many player currently in room whenever new player joined or leave
            roomCapacity.text = $"{networkGameManager.PlayersInRoom.Count} / {maxPlayer}";

            // Check if there's contents in player content master 
            if (contentMasterPlayer.childCount != 0)
            {
                // Adjust height of content
                RectTransform c = contentMasterPlayer.GetChild(contentMasterPlayer.childCount - 1).GetComponent<RectTransform>();
                float childHeight = c.sizeDelta.y + Math.Abs(-topPaddingEachContent);
                float heightContent = contentMasterPlayer.childCount * (childHeight);
                contentMasterPlayer.sizeDelta = new Vector2(contentMasterPlayer.sizeDelta.x, heightContent);

                // Adjust each player ui content
                for (int i = contentMasterPlayer.childCount; i > 0; i--)
                {
                    RectTransform playerNameDisplay = contentMasterPlayer.GetChild(i - 1).GetComponent<RectTransform>();
                    c.anchoredPosition = new Vector2(playerNameDisplay.anchoredPosition.x, -topPaddingEachContent - ((contentMasterPlayer.childCount - i) * childHeight));
                }
            }
        }
    }

}