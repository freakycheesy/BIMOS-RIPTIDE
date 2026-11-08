using UnityEngine;

namespace BIMOS.Netcode
{
    public enum ServerToClient : ushort
    {
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
