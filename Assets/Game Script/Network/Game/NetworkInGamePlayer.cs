using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BNEGame.Network
{
    public struct LocalPlayerPacket
    {
        public int hp;
        public float posX;
        public float posY;
        public float mousePosX;
        public float mousePosY;
        public int currentUse;
    }

    public class NetworkInGamePlayer : NetworkBehaviour, IHaveName
    {
        [SyncVar]
        [SerializeField] private string playerName = "Player";
        [SerializeField] private float positionSensitivity = 0.1f;

        [Space, Header("Requirement Attributes")]
        [SerializeField] private PlayerEntity _player = null;
        [SerializeField] private OnlineInputHandler _inputHandler = null;

        private SyncList<QuiverNode> syncQuiver = new SyncList<QuiverNode>();
        private NetworkGameRoomManager manager;

        #region Properties
        public string DisplayName
        {
            get => playerName;
            set => playerName = value;
        }
        public NetworkGameRoomManager GameRoomManager
        {
            get
            {
                if (manager != null) return manager;
                return manager = FindObjectOfType<NetworkGameRoomManager>();
            }
        }
        #endregion

        #region Unity BuiltIn Methods
        private void LateUpdate()
        {
            if (!isLocalPlayer)
                return;

            ClnUpdateLocalPlayerPacket();
        }

        public override void OnStartClient()
        {
            GameRoomManager.PlayersInGame.Add(this);

            // Sync player inventory
            _player.ArrowCollection.AddRange(syncQuiver);
        }

        public override void OnStopClient()
        {
            GameRoomManager.PlayersInGame.Remove(this);

            // Clear when client disconnected
            _player.ArrowCollection.Clear();
        }
        #endregion

        public void SyncCollectArrowInventory(int collectionIndex, ArrowTypes type, int amount)
        {
            // Handle Sync List
            if (_player.ArrowCollection.Count < syncQuiver.Count)
                syncQuiver.RemoveAt(collectionIndex);
            else if (_player.ArrowCollection.Count > syncQuiver.Count)
                syncQuiver.Add(new QuiverNode { Type = type, LeftoverAmount = amount });
            else
            {
                QuiverNode packet = syncQuiver[collectionIndex];
                syncQuiver[collectionIndex] = new QuiverNode
                {
                    Type = packet.Type,
                    LeftoverAmount = packet.LeftoverAmount + amount
                };
            }

            // Send collect item trigger to all client
            FindObjectOfType<ItemSpawnerInfoParser>()?.SyncItemOnSpawnerCollected(collectionIndex);
            RpcSyncQuiver(collectionIndex);
        }

        public void SyncPlayer(LocalPlayerPacket packet)
        {
            // Sync player HP
            if (packet.hp != _player.CurrentHealth)
                _player.AddHealth(packet.hp - _player.CurrentHealth);

            // Sync player transform
            Vector3 pos = new Vector3(packet.posX, packet.posY, transform.position.z);
            if (Vector3.Distance(transform.position, pos) > positionSensitivity)
                transform.position = pos;

            // Sync mouse cursor on movement position
            Vector2 currentMousePos = _inputHandler.LocalInputData.AimPosition;
            if (currentMousePos.x != packet.mousePosX || currentMousePos.y != packet.mousePosY)
                _inputHandler.LocalInputData.AimPosition = new Vector2(packet.mousePosX, packet.mousePosY);

            // Sync current arrow being used
            ArrowTypes type = (ArrowTypes)packet.currentUse;
            if (_player.CurrentArrowUse == ArrowTypes.None && _player.ArrowCollection.Count > 0)
                _player.CurrentArrowUse = type;
            else if (_player.CurrentArrowUse != ArrowTypes.None && _player.ArrowCollection.Count == 0)
                _player.CurrentArrowUse = ArrowTypes.None;
            else if (_player.CurrentArrowUse != type)
                _player.CurrentArrowUse = type;
        }

        public void SyncShootArrow(Vector2 shootDir)
        {
            GameRoomManager.GameMngr?.ServerParser.CmdPlayerShoot(netIdentity, _player.BowShootOrigin, shootDir);
        }

        [Client]
        private void ClnUpdateLocalPlayerPacket()
        {
            LocalPlayerPacket packet = new LocalPlayerPacket();
            packet.hp = _player.CurrentHealth;
            packet.posX = _player.transform.position.x;
            packet.posY = _player.transform.position.y;
            packet.mousePosX = _inputHandler.LocalInputData.AimPosition.x;
            packet.mousePosY = _inputHandler.LocalInputData.AimPosition.y;
            packet.currentUse = (int)_player.CurrentArrowUse;
            GameRoomManager.GameMngr?.ServerParser.CmdUpdateLocalPlayerPacket(netIdentity, packet);
        }

        [ClientRpc]
        public void RpcSyncQuiver(int editedIndex)
        {
            if (_player.ArrowCollection.Count < syncQuiver.Count)
            {
                QuiverNode packetAdd = syncQuiver[syncQuiver.Count - 1];
                _player.ArrowCollection.Add(new QuiverNode { Type = packetAdd.Type, LeftoverAmount = packetAdd.LeftoverAmount });
            }
            else if (_player.ArrowCollection.Count > syncQuiver.Count)
            {
                _player.ArrowCollection.RemoveAt(editedIndex);
            }
            else
            {
                if (editedIndex < _player.ArrowCollection.Count)
                {
                    QuiverNode dat = _player.ArrowCollection[editedIndex];
                    dat.LeftoverAmount = syncQuiver[editedIndex].LeftoverAmount;
                }
            }

            Debug.Log($"Have kinds of arrow: {_player.ArrowCollection.Count}; Current Use: {_player.CurrentArrowUse}");
        }
    }

}