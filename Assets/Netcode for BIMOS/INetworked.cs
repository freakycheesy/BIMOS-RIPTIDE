using LiteNetLib.Utils;

namespace BIMOS.Netcode
{
    /// <summary>
    /// Required for scripts that want to sync their state
    /// </summary>
    public interface INetworked
    {
        void OnSerialize(NetDataWriter stream);
        void OnDeserialize(NetDataReader stream);
    }
}
