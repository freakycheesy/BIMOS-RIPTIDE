using Riptide;
using UnityEngine;

namespace BIMOS.Netcode
{
    public static class NetworkServer
    {
        public static ushort CurrentTick { get; set; }

        internal static void FixedUpdate()
        {
            NetworkManager.Server.Update();
            if (NetworkManager.Server.IsRunning)
            {
                if (CurrentTick % 200 == 0)
                    SendSync();

                CurrentTick++;
            }
        }

        static void SendSync()
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClient.Sync).Add(CurrentTick);
            if (NetworkManager.Client.IsConnected) NetworkManager.Server.SendToAll(message, NetworkManager.Client.Id);
            else NetworkManager.Server.SendToAll(message);
        }

    }
}
