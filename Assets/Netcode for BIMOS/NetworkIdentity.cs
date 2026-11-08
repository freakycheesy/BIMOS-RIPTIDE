using LiteNetLib.Utils;
using Riptide;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS.Netcode
{
    public class NetworkIdentity : MonoBehaviour
    {
        public uint assetId;
        public NetworkBehaviour[] behaviours { get; set; }
        public int id { get; internal set; } = -1;
        public ushort ownerId { get; internal set; }
        public bool isMine => NetworkClient.Instance.IsConnected && ownerId == NetworkClient.Instance.Id;
        private Dictionary<byte, CommandCallback> commands { get; set; } = new();
        private Dictionary<byte, RPCCallback> rpcs { get; set; } = new();
        private void Awake()
        {
            var identities = GetComponentsInChildren<NetworkIdentity>(false);
            foreach(var identity in identities)
            {
                if (identity == this) continue;
                NetworkLevel.Instance.RegisterIdentity(identity, false);
            }
            behaviours = GetComponentsInChildren<NetworkBehaviour>();
            foreach(var behaviour in behaviours)
            {
                behaviour.identity = this;
                behaviour.RegisterRpcs(this);
            }
        }

        private void Start()
        {
            foreach (var behaviour in behaviours)
            {
                behaviour.OnStart();
            }
        }

        public void SetOwnership(Connection newOwner)
        {
            if (!NetworkServer.Instance.IsRunning) return;
            ownerId = newOwner.Id;
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.SetOwner);
            message.Add(id);
            message.Add(ownerId);
            if (NetworkClient.Instance.IsConnected) NetworkServer.Instance.SendToAll(message, NetworkClient.Instance.Id);
            else NetworkServer.Instance.SendToAll(message);
        }

        public void Serialize()
        {
            if (!NetworkClient.Instance.IsConnected) return;
            if (!isMine) return;
            Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ClientToServer.Serialize);
            message.Add(id);
            foreach (var behaviour in behaviours)
            {
                behaviour.OnSerialize(message);
            }
            NetworkClient.Instance.Send(message);
        }
        public void Deserialize(Message message)
        {
            if (!NetworkClient.Instance.IsConnected) return;
            foreach (var behaviour in behaviours)
            {
                behaviour.OnDeserialize(message);
            }
        }
        public byte RegisterCommand(CommandCallback commandCallback)
        {
            var command = (byte)commands.Count;
            commands.Add(command, commandCallback);
            return command;
        }
        public byte RegisterRpc(RPCCallback rpcCallback)
        {
            var rpc = (byte)rpcs.Count;
            rpcs.Add(rpc, rpcCallback);
            return rpc;
        }
        /// <summary>
        ///  Sends an rpc to a connection
        /// </summary>
        /// <param name="rpc">the rpc id</param>
        /// <param name="connection">the connection receiving the rpc</param>
        /// <param name="data">the data</param>
        /// <param name="sendMode">reliablility</param>
        public void SendTargetRpc(byte rpc, Connection connection, MessageSendMode sendMode = MessageSendMode.Unreliable, params object[] objects)
        {
            if (!NetworkServer.Instance.IsRunning) return;
            Message message = Message.Create(sendMode, (ushort)ServerToClient.Rpc);
            message.Add(id);
            message.Add(rpc);
            if (objects != null && objects.Length > 0) {
                NetDataWriter writer = new();
                foreach(var obj in objects)
                {
                    writer.Serialize(obj);
                }
                message.Add(writer.CopyData());
            }
            connection.Send(message);
        }
        /// <summary>
        /// Sends an rpc to everyone
        /// </summary>
        /// <param name="rpc">the rpc id</param>
        /// <param name="data">the data</param>
        /// <param name="sendMode">reliablility</param>
        public void SendClientRpc(byte rpc, MessageSendMode sendMode = MessageSendMode.Unreliable, params object[] objects)
        {
            if (!NetworkServer.Instance.IsRunning) return;
            Message message = Message.Create(sendMode, (ushort)ServerToClient.Rpc);
            message.Add(id);
            message.Add(rpc);
            if (objects != null && objects.Length > 0)
            {
                NetDataWriter writer = new();
                foreach (var obj in objects)
                {
                    writer.Serialize(obj);
                }
                message.Add(writer.CopyData());
            }
            NetworkServer.Instance.SendToAll(message);
        }
        /// <summary>
        /// Sends an rpc to everyone
        /// </summary>
        /// <param name="rpc">the rpc id</param>
        /// <param name="data">the data</param>
        /// <param name="sendMode">reliablility</param>
        public void SendClientRpc(byte rpc, Connection excluded, MessageSendMode sendMode = MessageSendMode.Unreliable, params object[] objects)
        {
            if (!NetworkServer.Instance.IsRunning) return;
            Message message = Message.Create(sendMode, (ushort)ServerToClient.Rpc);
            message.Add(id);
            message.Add(rpc);
            if (objects != null && objects.Length > 0)
            {
                NetDataWriter writer = new();
                foreach (var obj in objects)
                {
                    writer.Serialize(obj);
                }
                message.Add(writer.CopyData());
            }
            NetworkServer.Instance.SendToAll(message, excluded.Id);
        }
        /// <summary>
        /// Sends an rpc to the Instance
        /// </summary>
        /// <param name="command">the rpc id</param>
        /// <param name="data">the data</param>
        /// <param name="sendMode">reliablility</param>
        public void SendCommand(byte command, Message data = null, MessageSendMode sendMode = MessageSendMode.Unreliable)
        {
            if (!NetworkClient.Instance.IsConnected) return;
            Message message = Message.Create(sendMode, (ushort)ClientToServer.Command);
            message.Add(id);
            message.Add(command);
            if (data != null) message.AddMessage(data);
            NetworkClient.Instance.Send(message);
        }
        public void Spawn()
        {
            if (!NetworkServer.Instance.IsRunning) return;
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClient.Spawn);
            message.Add(assetId);
            message.Add(ownerId);
            message.Add(transform.position);
            message.Add(transform.rotation);
            if (NetworkClient.Instance.IsConnected) NetworkServer.Instance.SendToAll(message, NetworkClient.Instance.Id);
            else NetworkServer.Instance.SendToAll(message);
            NetworkLevel.Instance.RegisterIdentity(this, true);
        }
        public void Despawn(bool destroy = true)
        {
            if (!NetworkServer.Instance.IsRunning) return;
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClient.Despawn);
            message.Add(id);
            message.Add(destroy);
            if (NetworkClient.Instance.IsConnected) NetworkServer.Instance.SendToAll(message, NetworkClient.Instance.Id);
            else NetworkServer.Instance.SendToAll(message);
            if (destroy) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
        [MessageHandler((ushort)ServerToClient.Spawn)]
        static void HandleSpawn(Message received)
        {
            var instance = Instantiate(NetworkManager.Instance.Prefabs[received.GetUShort()]);
            instance.GetComponent<NetworkIdentity>().ownerId = received.GetUShort();
            instance.transform.position = received.GetVector3();
            instance.transform.rotation = received.GetQuaternion();
        }
        [MessageHandler((ushort)ServerToClient.Despawn)]
        static void HandleDespawn(Message received)
        {
            var identity = NetworkLevel.Instance.identities[received.GetInt()];
            if (identity == null) return;
            if (received.GetBool()) Destroy(identity.gameObject);
            else identity.gameObject.SetActive(false);
        }
        [MessageHandler((ushort)ClientToServer.Serialize)]
        static void HandleSerialize(ushort sender, Message received)
        {
            var identity = NetworkLevel.Instance.identities[received.GetInt()];
            if (identity == null) return;
            if (identity.ownerId != sender) return;
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClient.Serialize);
            message.Add(identity.id);
            message.AddMessage(received);
            NetworkServer.Instance.SendToAll(message, sender);
        }
        [MessageHandler((ushort)ServerToClient.Serialize)]
        static void HandleSerialize(Message received)
        {
            var identity = NetworkLevel.Instance.identities[received.GetInt()];
            if (identity == null) return;
            identity.Deserialize(received);
        }

        [MessageHandler((ushort)ClientToServer.Command)]
        static void HandleCommand(ushort sender, Message received)
        {
            if (!NetworkServer.Instance.TryGetClient(sender, out Riptide.Connection connection)) return;
            var identity = NetworkLevel.Instance.identities[received.GetInt()];
            if(identity == null) return;
            identity.commands[received.GetByte()].Invoke(connection, new(received.GetBytes()));       
        }

        [MessageHandler((ushort)ServerToClient.Rpc)]
        static void HandleRpc(Message received)
        {
            var identity = NetworkLevel.Instance.identities[received.GetInt()];
            if (identity == null) return;
            identity.rpcs[received.GetByte()].Invoke(new(received.GetBytes()));
        }

        [MessageHandler((ushort)ServerToClient.SetOwner)]
        static void HandleSetOwner(Message received)
        {
            var identity = NetworkLevel.Instance.identities[received.GetInt()];
            if (identity == null) return;
            identity.ownerId = received.GetUShort();
        }
    } 
}
