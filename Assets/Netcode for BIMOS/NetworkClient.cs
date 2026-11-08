using Riptide;
using UnityEngine;

namespace BIMOS.Netcode
{
    public static class NetworkClient
    {
        private static ushort _serverTick;
        public static ushort ServerTick
        {
            get => _serverTick;
            private set
            {
                _serverTick = value;
                InterpolationTick = (ushort)(value - TicksBetweenPositionUpdates);
            }
        }
        public static ushort InterpolationTick { get; private set; }
        private static ushort _ticksBetweenPositionUpdates = 2;
        public static ushort TicksBetweenPositionUpdates
        {
            get => _ticksBetweenPositionUpdates;
            private set
            {
                _ticksBetweenPositionUpdates = value;
                InterpolationTick = (ushort)(ServerTick - value);
            }
        }
        private static ushort tickDivergenceTolerance = 1;
        public static Client Instance { get; set; } = new();
        public static void Connect(string address, ushort port)
        {
            Instance.Connect($"{address}:{port}");
        }
        internal static void FixedUpdate()
        {
            Instance.Update();
        }
        static void SetTick(ushort serverTick)
        {
            if (Mathf.Abs(ServerTick - serverTick) > tickDivergenceTolerance)
            {
                Debug.Log($"Instance tick: {ServerTick} -> {serverTick}");
                ServerTick = serverTick;
            }
        }


        [MessageHandler((ushort)ServerToClient.Sync)]
        static void HandleSync(Message message)
        {
            SetTick(message.GetUShort());
        }
    }
}
