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

        private void RpcOnEvent(Message reader)
        {
            Event?.Invoke(reader.GetUShort());
        }

        private void CmdOnEvent(Connection sender, Message reader)
        {
            foreach(var connection in NetworkManager.Instance.server.Clients)
            {
                if(allowDesync && connection == sender) continue;
                identity.SendTargetRpc(RPC_EVENT, connection, Message.Create().Add(sender.Id), MessageSendMode.Reliable);
            }
        }

        public void Invoke()
        {
            if (!NetworkManager.Instance.client.IsConnected) {
                Event?.Invoke(0);
                return;
            } 
            if(allowDesync) Event?.Invoke(NetworkManager.Instance.client.Id);
            identity.SendCommand(CMD_EVENT, sendMode:MessageSendMode.Reliable);
        }
    }
}
