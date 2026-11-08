using Riptide;
using Riptide.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS.Netcode
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        public Server server { get; set; }
        public Client client { get; set; }
        [SerializeField] private GameObject[] _prefabs = new GameObject[256];
        public Dictionary<uint, GameObject> prefabs = new();
        private void Start()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            prefabs.Clear();
            RiptideLogger.Initialize(Debug.Log, false);
            server = new();
            client = new();
            foreach (var prefab in _prefabs)
            {
                if(prefab.TryGetComponent(out NetworkIdentity identity))
                {
                    prefabs.Add(identity.assetId, prefab);
                }
                else
                {
                    Debug.LogWarning("Prefab may not have NetworkIdenitity Component", prefab);
                }
            }
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            server.Update();
            client.Update();
        }
    }
}
