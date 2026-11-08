using LiteNetLib.Utils;
using Riptide;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace BIMOS.Netcode
{
    public class NetworkEvent : NetworkBehaviour
    {
        public UnityEvent<ushort> Event;
        public bool allowDesync = true;
        private byte CMD_EVENT;
        private byte RPC_EVENT;
        public override void RegisterRpcs(NetworkIdentity identity)
        {
            base.RegisterRpcs(identity);
            CMD_EVENT = identity.RegisterCommand(CmdOnEvent);
            RPC_EVENT = identity.RegisterRpc(RpcOnEvent);
            
        }

        private void RpcOnEvent(NetDataReader reader)
        {
            Event?.Invoke(reader.GetUShort());
        }

        private void CmdOnEvent(Connection sender, NetDataReader reader)
        {
            foreach(var connection in NetworkServer.Instance.Clients)
            {
                if(allowDesync && connection == sender) continue;
                identity.SendTargetRpc(RPC_EVENT, connection, MessageSendMode.Reliable, sender.Id);
            }
        }

        public void Invoke()
        {
            if (!NetworkClient.Instance.IsConnected) {
                Event?.Invoke(0);
                return;
            } 
            if(allowDesync) Event?.Invoke(NetworkClient.Instance.Id);
            identity.SendCommand(CMD_EVENT, sendMode:MessageSendMode.Reliable);
        }
    }
}
