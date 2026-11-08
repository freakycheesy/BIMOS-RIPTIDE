using Riptide;
using UnityEngine;

namespace BIMOS.Netcode
{
    public class NetworkBehaviour : MonoBehaviour
    {
        public NetworkIdentity identity { get; internal set; }

        public virtual void RegisterRpcs(NetworkIdentity identity)
        {

        }
        public virtual void OnStart()
        {

        }

        public virtual void OnSerialize(Message stream)
        {

        }

        public virtual void OnDeserialize(Message stream)
        {
        }
    }
}
