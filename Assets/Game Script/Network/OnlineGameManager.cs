using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum MultiplayerGameMode
{
    None = 0,
    DeathMatch
}

public class OnlineGameManager : NetworkManager
{
    private struct CreatePlayerMsg : NetworkMessage
    {
        public string name;
    }

    [Header("Online Game System Attributes")]
    [SerializeField] private string _username = "";
    [SerializeField] private float _defaultRespawnTime = 3f;
    [SerializeField] private RectTransform _gamePanel = null;
    [SerializeField] private ChatBoxMessager _messager = null;
    [SerializeField] private MultiplayerGameMode _mode = MultiplayerGameMode.None;

    public MultiplayerGameMode Mode => _mode;

    #region Unity BuiltIn Methods
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<CreatePlayerMsg>(OnCreatePlayer);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        // Activate messager
        _messager.OpenChatBox(true);

        // tell the server to create a player with this name
        conn.Send(new CreatePlayerMsg { name = _username });

        //if (_host == null && conn.identity != null)
        //    _host = conn.identity.GetComponent<PlayerEntity>();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        //if (_host != null && conn.identity != null)
        //    if (conn.identity.GetComponent<PlayerEntity>().Equals(_host))
        //        _host = null;
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
        switch (Mode)
        {
            case MultiplayerGameMode.DeathMatch:
                if (args.EntityVictim is PlayerEntity)
                    StartCoroutine(PlayerRespawnDelay((PlayerEntity)args.EntityVictim, _defaultRespawnTime));
                break;
        }
    }

    private void OnCreatePlayer(NetworkConnection connection, CreatePlayerMsg createPlayerMessage)
    {
        // create a gameobject using the name supplied by client
        GameObject playergo = Instantiate(playerPrefab);

        PlayerNetworkInformation player = playergo.GetComponent<PlayerNetworkInformation>();
        if (player != null)
            player.Username = createPlayerMessage.name;

        // set it as the player
        if (NetworkServer.AddPlayerForConnection(connection, playergo))
        {
            
        }

        _messager.gameObject.SetActive(true);
    }
    #endregion

    public void SetUpGame(MultiplayerGameMode mode)
    {
        switch (mode)
        {
            case MultiplayerGameMode.DeathMatch:
                break;
        }
    }
    private IEnumerator PlayerRespawnDelay(PlayerEntity player, float timeDelay)
    {
        float t = timeDelay;
        while (t > 0)
        {
            yield return null;
            t -= Time.deltaTime;
        }

        //_host.gameObject.SetActive(true);
    }

    public RectTransform GetDefaultGamePanel()
    {
        return _gamePanel;
    }
}
