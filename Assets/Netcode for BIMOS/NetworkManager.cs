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
        public static NetworkManager Instance { get; private set; }

        [SerializeField] private GameObject[] _prefabs = new GameObject[256];
        public Dictionary<uint, GameObject> Prefabs { get; } = new();
        private void Start()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Prefabs.Clear();
            RiptideLogger.Initialize(Debug.Log, false);
            foreach (var prefab in _prefabs)
            {
                if(prefab.TryGetComponent(out NetworkIdentity identity))
                {
                    Prefabs.Add(identity.assetId, prefab);
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
