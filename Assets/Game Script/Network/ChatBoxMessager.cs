using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Mirror;

public class ChatBoxMessager : NetworkBehaviour, IPointerDownHandler
{
    [SerializeField] private ScrollRect _vpScroller = null;
    [SerializeField] private RectTransform _contentMaster = null;
    [SerializeField] private Text _msgPrefab = null;
    [SerializeField] private InputField _inputField = null;

    private List<string> _messages = new List<string>();

    public string TxtOnInput => _inputField.text;

    #region Unity BuiltIn Methods
    private void Awake()
    {
        // Subscribe technical event
        PlayerNetworkInformation.OnPlayerSendMsgEvent += OnSendMsg;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        OpenChatBox(true);
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        OpenChatBox(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe technical event
        PlayerNetworkInformation.OnPlayerSendMsgEvent -= OnSendMsg;
    }
    #endregion

    #region Event Methods
    public void OnPointerDown(PointerEventData eventData)
    {
        OpenChatBox(true);
    }

    private void OnSendMsg(PlayerNetworkInformation player, string msg)
    {
        Text newTxt = Instantiate(_msgPrefab, _contentMaster);
        newTxt.text = $"{player.Username}: {msg}";

        newTxt.color = isLocalPlayer ? Color.cyan : Color.white;
    }
    #endregion

    public void SendChatMessage()
    {
        if (_inputField.text.Trim() == "")
            return;

        // get our player
        PlayerNetworkInformation player = NetworkClient.connection.identity.GetComponent<PlayerNetworkInformation>();

        // send a message
        player.CmdSendChatMessage(_inputField.text.Trim());

        _inputField.text = "";
    }

    public void ClearTxtOnInput()
    {
        _inputField.text = "";
    }

    public void OpenChatBox(bool open)
    {
        _vpScroller.gameObject.SetActive(open);
    }
}
