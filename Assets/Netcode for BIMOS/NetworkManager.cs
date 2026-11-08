using Riptide;
using Riptide.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS.Netcode
{
    [DefaultExecutionOrder(-100)]
    public class NetworkManager : MonoBehaviour
    {
        public static Server Server { get; set; } = new();
        public static Client Client { get; set; } = new();

        public static NetworkManager Instance { get; private set; }

        [SerializeField] public List<GameObject> spawnPrefabs;
        public Dictionary<uint, GameObject> SpawnPrefabs { get; } = new();
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            SpawnPrefabs.Clear();
            RiptideLogger.Initialize(Debug.Log, false);
            foreach (var prefab in spawnPrefabs)
            {
                if(prefab.TryGetComponent(out NetworkIdentity identity))
                {
                    SpawnPrefabs.Add(identity.assetId, prefab);
                }
                else
                {
                    Debug.LogWarning("Prefab may not have NetworkIdenitity Component", prefab);
                }
            }
            DontDestroyOnLoad(gameObject);
        }

        private void FixedUpdate()
        {
            NetworkServer.FixedUpdate();
            NetworkClient.FixedUpdate();        
        }
    }
}
