using Riptide;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS.Netcode
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        public Server server { get; set; }
        public Client client { get; set; }
        [SerializeField] private GameObject[] _prefabs;
        public Dictionary<uint, GameObject> prefabs = new();
        private void Start()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            server = new();
            client = new();
            foreach(var prefab in _prefabs)
            {
                prefabs.Add(prefab.GetComponent<NetworkIdentity>().assetId, prefab);
            }
            DontDestroyOnLoad(gameObject);
        }
    }
}
