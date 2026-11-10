using LiteNetLib.Utils;
using Riptide;
using System;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.Events;

namespace BIMOS.Netcode
{
    public class NetworkEvent : MonoBehaviour, IRpc
    {
        public NetworkIdentity netIdentity;
        public UnityEvent<ushort> Event;
        public bool allowDesync = true;
        [Rpc(RpcTargets.Client)]
        private void RpcOnEvent(ushort sender)
        {
            Event?.Invoke(sender);
        }
        [Rpc(RpcTargets.Server)]
        private void CmdOnEvent(ushort sender)
        {
            if(allowDesync)netIdentity.SendClientRpc(nameof(RpcOnEvent), sender, objects: sender);
            else netIdentity.SendClientRpc(nameof(RpcOnEvent), objects: sender);
        }
        public void Invoke()
        {
            if (!NetworkManager.Client.IsConnected) {
                Event?.Invoke(0);
                return;
            } 
            if(allowDesync) Event?.Invoke(NetworkManager.Client.Id);
            netIdentity.SendServerRpc(nameof(CmdOnEvent));
        }
    }
}
