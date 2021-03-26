using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworkInformation : NetworkBehaviour
{
    public delegate void PlayerSendMsg(PlayerNetworkInformation player, string msg);
    public static event PlayerSendMsg OnPlayerSendMsgEvent;


    public struct LocalPlayerPacket
    {
        public int _hp;
        public float _posX;
        public float _posY;
        public float _mousePosX;
        public float _mousePosY;
    }


    [SyncVar]
    [SerializeField] private string _username = "Player";

    [Header("Requirement Attributes")]
    [SerializeField] private PlayerEntity _player = null;
    [SerializeField] private OnlineInputHandler _inputHandler = null;

    [SyncVar]
    private LocalPlayerPacket _syncPlayerPacket = new LocalPlayerPacket();

    public PlayerEntity Player => _player;
    public string Username
    {
        set => _username = value;
        get => _username;
    }


    #region Unity BuiltIn Methods
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        // Handle player send chat message
        if (_inputHandler.LocalInputData.ReturnEnterPressed)
        {
            ChatBoxMessager chatBox = FindObjectOfType<ChatBoxMessager>();
            chatBox.SendChatMessage();
        }
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer)
            return;

        ClnUpdateLocalPlayerPacket();
    }
    #endregion

    [Client]
    private void ClnUpdateLocalPlayerPacket()
    {
        LocalPlayerPacket packet = new LocalPlayerPacket();
        packet._hp = _player.CurrentHealth;
        packet._posX = _player.transform.position.x;
        packet._posY = _player.transform.position.y;
        packet._mousePosX = _inputHandler.LocalInputData.AimPosition.x;
        packet._mousePosY = _inputHandler.LocalInputData.AimPosition.y;
        CmdUpdateLocalPlayerPacket(packet);
    }


    [Command]
    public void CmdUpdateLocalPlayerPacket(LocalPlayerPacket packet)
    {
        _syncPlayerPacket = packet;
        RpcReceivePlayerPacket(packet);
    }

    [Command]
    public void CmdSendChatMessage(string message)
    {
        if (message.Trim() != "")
            RpcReceiveMessage(message.Trim());
    }


    [ClientRpc]
    private void RpcReceivePlayerPacket(LocalPlayerPacket packet)
    {
        if (packet._hp != _player.CurrentHealth)
            _player.AddHealth(packet._hp - _player.CurrentHealth);

        Vector3 currentPos = _player.transform.position;
        if (currentPos.x != packet._posX || currentPos.y != packet._posY)
            _player.transform.position = new Vector3(packet._posX, packet._posY, _player.transform.position.z);

        Vector2 currentMousePos = _inputHandler.LocalInputData.AimPosition;
        if (currentMousePos.x != packet._mousePosX || currentMousePos.y != packet._mousePosY)
            _inputHandler.LocalInputData.AimPosition = new Vector2(packet._mousePosX, packet._mousePosY);
    }

    [ClientRpc]
    private void RpcReceiveMessage(string message)
    {
        OnPlayerSendMsgEvent?.Invoke(this, message);
    }
}
