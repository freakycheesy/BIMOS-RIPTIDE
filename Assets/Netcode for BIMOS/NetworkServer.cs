using Riptide;
using UnityEngine;

namespace BIMOS.Netcode
{
    public static class NetworkServer
    {
        public static Server Instance { get; set; } = new();
        public static ushort CurrentTick { get; set; }

        public static void Listen(ushort port = 7777, ushort maxConnections = 32, bool startClient = true)
        {
            Instance.Start(port, maxConnections);
            if (startClient) NetworkClient.Connect("localhost", port);
        }
        internal static void FixedUpdate()
        {
            Instance.Update();
            if (Instance.IsRunning)
            {
                if (CurrentTick % 200 == 0)
                    SendSync();

                CurrentTick++;
            }
        }

        static void SendSync()
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerToClient.Sync).Add(CurrentTick);
            if (NetworkClient.Instance.IsConnected) Instance.SendToAll(message, NetworkClient.Instance.Id);
            else Instance.SendToAll(message);
        }

    }
}
