using LiteNetLib.Utils;
using Newtonsoft.Json;
using Riptide;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BIMOS.Netcode
{
    public sealed class NetworkIdentity : MonoBehaviour
    {
        public uint assetId;
        private IRpc[] rpcBehaviours { get; set; }
        private INetworked[] networkedBehaviours { get; set; }
        public int netId { get; internal set; } = -1;
        public ushort ownerId { get; internal set; }
        public bool isMine => NetworkManager.Client.IsConnected && ownerId == NetworkManager.Client.Id;
        private Dictionary<string, RpcMethod> rpcs { get; set; } = new();
        public bool autoSync = true;
        private struct RpcMethod
        {
            public IRpc Component { get; private set; }
            public RpcTargets Targets { get; private set; }
            public MethodInfo Method { get; private set; }
            public RpcMethod(IRpc component, RpcTargets target, MethodInfo method)
            {
                Component = component;
                Targets = target;
                Method = method;
            }
        }
        private void Awake()
        {
            var identities = GetComponentsInChildren<NetworkIdentity>(false);
            foreach (var identity in identities)
            {
                if (identity == this) continue;
                NetworkSceneManager.Instance.RegisterIdentity(identity, false);
            }
            networkedBehaviours = GetComponentsInChildren<INetworked>();
            rpcBehaviours = GetComponentsInChildren<IRpc>();
            RegisterRpcs();
        }

        private void RegisterRpcs()
        {
            rpcs.Clear();
            foreach (IRpc rpcBehaviour in rpcBehaviours)
            {
                foreach (var method in rpcBehaviour.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var rpc = method.GetCustomAttribute<RpcAttribute>();
                    if (rpc == null) continue;
                    rpcs.Add(method.Name, new RpcMethod(rpcBehaviour, rpc.Type, method));
                }
            }
        }

        public void SetOwnership(Connection newOwner)
        {
            SetOwnership(newOwner.Id);
        }

        public void SetOwnership(ushort newOwner)
        {
            if (!NetworkManager.Server.IsRunning) return;
            ownerId = newOwner;
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerToClient.SetOwner);
            message.Add(netId);
            message.Add(ownerId);
            if (NetworkManager.Client.IsConnected) NetworkManager.Server.SendToAll(message, NetworkManager.Client.Id);
            else NetworkManager.Server.SendToAll(message);
        }
        private void FixedUpdate()
        {
            if (isMine && autoSync && NetworkClient.ServerTick % 200 == 0) SerializeView();
        }
        NetDataWriter writer = new();
        public void SerializeView()
        {
            if (!NetworkManager.Client.IsConnected) return;
            if (!isMine) return;
            Message message = Message.Create(MessageSendMode.Unreliable, (ushort)ClientToServer.Serialize);
            message.Add(netId);
            writer.Reset();
            foreach (var networked in networkedBehaviours)
            {
                networked.OnSerialize(writer);
            }
            message.Add(writer.CopyData());
            NetworkManager.Client.Send(message);
        }
        public void Deserialize(NetDataReader reader)
        {
            if (!NetworkManager.Client.IsConnected) return;
            foreach (var networked in networkedBehaviours)
            {
                networked.OnDeserialize(reader);
            }
        }

        /// <summary>
        /// Sends an rpc to everyone
        /// </summary>
        /// <param name="rpc">the rpc id</param>
        /// <param name="objects">data passed</param>
        /// <param name="reliable">is reliable</param>
        /// <param name="exceptPlayer">this player will not receive the message</param>
        public void SendClientRpc(string rpc, ushort? exceptPlayer = null, bool reliable = false, params object[] objects)
        {
            if (!NetworkManager.Server.IsRunning) return;
            Message message = Message.Create(reliable ? MessageSendMode.Reliable : MessageSendMode.Unreliable, ServerToClient.Rpc);
            message.AddRpcMessage(netId, rpc, objects);
            if (exceptPlayer == null) NetworkManager.Server.SendToAll(message);
            else NetworkManager.Server.SendToAll(message, exceptPlayer.GetValueOrDefault());
        }
        /// <summary>
        /// Sends an rpc to a target
        /// </summary>
        /// <param name="rpc">the rpc id</param>
        /// <param name="objects">data passed</param>
        /// <param name="reliable">is reliable</param>
        /// <param name="target">this player will only receive the message</param>
        public void SendTargetRpc(string rpc, ushort target, bool reliable = false, params object[] objects)
        {
            if (!NetworkManager.Server.IsRunning) return;
            var message = Message.Create(reliable ? MessageSendMode.Reliable : MessageSendMode.Unreliable, ServerToClient.Rpc);
            message.AddRpcMessage(netId, rpc, objects);
            NetworkManager.Server.Send(message, target);
        }
        /// <summary>
        /// Sends an rpc to the server
        /// </summary>
        /// <param name="rpc">the rpc id</param>
        /// <param name="objects">data passed</param>
        /// <param name="reliable">is reliable</param>
        public void SendServerRpc(string rpc, bool reliable = false, params object[] objects)
        {
            if (NetworkManager.Client.IsNotConnected) return;
           var message = Message.Create(reliable ? MessageSendMode.Reliable : MessageSendMode.Unreliable, ClientToServer.Rpc);
            message.AddRpcMessage(netId, rpc, objects);
            NetworkManager.Client.Send(message);
        }

        public void Spawn()
        {
            if (!NetworkManager.Server.IsRunning) return;
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClient.Spawn);
            message.Add(assetId);
            message.Add(ownerId);
            message.Add(transform.position);
            message.Add(transform.rotation);
            if (NetworkManager.Client.IsConnected) NetworkManager.Server.SendToAll(message, NetworkManager.Client.Id);
            else NetworkManager.Server.SendToAll(message);
            NetworkSceneManager.Instance.RegisterIdentity(this, true);
        }
        public void Despawn(bool destroy = true)
        {
            if (!NetworkManager.Server.IsRunning) return;
            Message message = Message.Create(MessageSendMode.Reliable, ServerToClient.Despawn);
            message.Add(netId);
            message.Add(destroy);
            if (NetworkManager.Client.IsConnected) NetworkManager.Server.SendToAll(message, NetworkManager.Client.Id);
            else NetworkManager.Server.SendToAll(message);
            if (destroy) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
        [MessageHandler((ushort)ServerToClient.Spawn)]
        static void HandleSpawn(Message received)
        {
            var instance = Instantiate(NetworkManager.Instance.SpawnPrefabs[received.GetUShort()]);
            instance.GetComponent<NetworkIdentity>().ownerId = received.GetUShort();
            instance.transform.position = received.GetVector3();
            instance.transform.rotation = received.GetQuaternion();
        }
        [MessageHandler((ushort)ServerToClient.Despawn)]
        static void HandleDespawn(Message received)
        {
            var identity = NetworkSceneManager.Instance.identities[received.GetInt()];
            if (identity == null) return;
            if (received.GetBool()) Destroy(identity.gameObject);
            else identity.gameObject.SetActive(false);
        }
        [MessageHandler((ushort)ClientToServer.Serialize)]
        static void HandleSerialize(ushort sender, Message received)
        {
            var identity = NetworkSceneManager.Instance.identities[received.GetInt()];
            if (identity == null) return;
            if (identity.ownerId != sender) return;
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClient.Serialize);
            message.Add(identity.netId);
            message.AddMessage(received);
            NetworkManager.Server.SendToAll(message, sender);
        }
        [MessageHandler((ushort)ServerToClient.Serialize)]
        static void HandleSerialize(Message received)
        {
            var identity = NetworkSceneManager.Instance.identities[received.GetInt()];
            if (identity == null) return;
            identity.Deserialize(new(received.GetBytes()));
        }

        [MessageHandler((ushort)ClientToServer.Rpc)]
        static void HandleRpc(ushort sender, Message received)
        {
            int netId = received.GetInt();
            string rpc = received.GetString();
            bool hasPararms = received.GetBool();
            if (!NetworkSceneManager.Instance.identities.ContainsKey(netId)) return;
            var identity = NetworkSceneManager.Instance.identities[netId];
            var rpcMethod = identity.rpcs[rpc];
            if (rpcMethod.Targets != RpcTargets.Server) return;
            var parameters = new object[] { sender };
            if (hasPararms)
            {
                parameters.Concat(JsonConvert.DeserializeObject<object[]>(received.GetString()));
                rpcMethod.Method.Invoke(rpcMethod.Component, parameters);
            }
            else
            {
                rpcMethod.Method.Invoke(rpcMethod.Component, parameters);
            }
        }

        [MessageHandler((ushort)ServerToClient.Rpc)]
        static void HandleRpc(Message received)
        {
            int netId = received.GetInt();
            string rpc = received.GetString();
            bool hasPararms = received.GetBool();
            if (!NetworkSceneManager.Instance.identities.ContainsKey(netId)) return;
            var identity = NetworkSceneManager.Instance.identities[netId];
            var rpcMethod = identity.rpcs[rpc];
            if (rpcMethod.Targets == RpcTargets.Server) return;
            if (hasPararms)
            {
                rpcMethod.Method.Invoke(rpcMethod.Component, JsonConvert.DeserializeObject<object[]>(received.GetString()));
            }
            else
            {
                rpcMethod.Method.Invoke(rpcMethod.Component, null);
            }
        }

        [MessageHandler((ushort)ServerToClient.SetOwner)]
        static void HandleSetOwner(Message received)
        {
            var identity = NetworkSceneManager.Instance.identities[received.GetInt()];
            if (identity == null) return;
            identity.ownerId = received.GetUShort();
        }
    }
}
