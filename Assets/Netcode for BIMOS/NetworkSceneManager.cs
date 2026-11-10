using System;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS.Netcode
{
    [DefaultExecutionOrder(-99)]
    public class NetworkSceneManager : MonoBehaviour
    {
        [SerializeField] private NetworkIdentity[] _identities;
        public Dictionary<int, NetworkIdentity> identities { get; private set; } = new Dictionary<int, NetworkIdentity>();
        public Dictionary<int, NetworkIdentity> spawned { get; private set; } = new Dictionary<int, NetworkIdentity>();
        public static NetworkSceneManager Instance { get; private set; }
        private void Awake()
        {
            identities.Clear();
            spawned.Clear();
            Instance = this;
            foreach(var identity in _identities)
            {
                RegisterIdentity(identity, false);
            }
        }

        public void RegisterIdentity(NetworkIdentity identity, bool isSpawned)
        {
            identity.netId = identities.Count;
            identities.Add(identity.netId, identity);
            if(isSpawned) spawned.Add(identity.netId, identity);
        }

        [ContextMenu("Find NetworkIdentites and Register")]
        public void FindIdentities()
        {
            _identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        }
    }
}