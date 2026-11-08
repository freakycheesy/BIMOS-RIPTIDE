using System;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS.Netcode
{
    [DefaultExecutionOrder(-99)]
    public class NetworkLevel : MonoBehaviour
    {
        [SerializeField] private NetworkIdentity[] _identities;
        public Dictionary<int, NetworkIdentity> identities { get; private set; } = new Dictionary<int, NetworkIdentity>();
        public Dictionary<int, NetworkIdentity> spawned { get; private set; } = new Dictionary<int, NetworkIdentity>();
        public static NetworkLevel Instance { get; private set; }
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
            identity.id = identities.Count;
            identities.Add(identity.id, identity);
            if(isSpawned) spawned.Add(identity.id, identity);
        }

        [ContextMenu("Find NetworkIdentites and Register")]
        public void FindIdentities()
        {
            _identities = FindObjectsByType<NetworkIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        }
    }
}