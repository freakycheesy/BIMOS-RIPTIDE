using UnityEngine;

namespace BIMOS.Netcode
{
    public enum ServerToClient : ushort
    {
        Sync,
        Rpc,
        SetOwner,
        Spawn,
        Despawn,
        Serialize,
    }
    public enum ClientToServer : ushort
    {
        Command,
        Serialize,
    }
}
