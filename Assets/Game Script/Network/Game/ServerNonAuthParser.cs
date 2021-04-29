using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace BNEGame.Network
{
    public class ServerNonAuthParser : NetworkBehaviour
    {
        [SerializeField] private ChatBoxMessager chatBox = null;
        [SerializeField] private ItemSpawnerInfoParser itemSpawnerParser = null;

        [Command(requiresAuthority = false)]
        public void CmdUpdateLocalPlayerPacket(NetworkIdentity identity, LocalPlayerPacket packet)
        {
            NetworkInGamePlayer player = identity.GetComponent<NetworkInGamePlayer>();

            if (player)
            {
                if (isServer && !isClient)
                    player.SyncPlayer(packet);

                RpcReceivePlayerPacket(player, packet);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdSendChatMessage(string msg, Color color, bool isSystemMsg)
        {
            if (isSystemMsg)
            {
                RpcSendChatMessage(msg, color);
                return;
            }

            MsgSentEventArgs arg = new MsgSentEventArgs(msg, color);
            EventHandler.CallEvent(arg);
            if (!arg.IsCancelled)
            {
                // Check message or command
                RpcSendChatMessage(msg, color);
            }
        }

        [Command(requiresAuthority = false)]
        public void CmdPlayerShoot(NetworkIdentity identity, Vector2 origin, Vector2 shootDir)
        {
            RpcSyncPlayerShoot(identity, origin, shootDir);
        }

        [TargetRpc]
        private void RpcSendPrivateMessage(NetworkConnection target, string msg, Color color)
        {
            chatBox.SendMessageToChatBox(new MsgLine { msg = msg, color = color }, true);
        }

        [ClientRpc]
        private void RpcSendChatMessage(string msg, Color color)
        {
            chatBox.SendMessageToChatBox(new MsgLine { msg = msg, color = color }, true);
        }

        [ClientRpc]
        private void RpcSyncPlayerShoot(NetworkIdentity identity, Vector2 origin, Vector2 shootDir)
        {
            identity.GetComponent<PlayerEntity>()?.BowShoot(shootDir, origin, true);
        }

        [ClientRpc]
        public void RpcFloatingItemWasCollected(int floatingItemIndex)
        {
            itemSpawnerParser.SyncItemOnSpawnerCollected(floatingItemIndex);
        }

        [ClientRpc]
        private void RpcReceivePlayerPacket(NetworkInGamePlayer player, LocalPlayerPacket packet)
        { 
            // Prevent update at client who sent to itself
            if (player.isLocalPlayer)
                return;

            // Sync player packet
            player.SyncPlayer(packet);
        }
    }
}
