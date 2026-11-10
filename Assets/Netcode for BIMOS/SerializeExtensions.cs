using LiteNetLib.Utils;
using Newtonsoft.Json;
using Riptide;
using Riptide.Utils;
using UnityEngine;

namespace BIMOS.Netcode
{
    public static class SerializeExtensions
    {
        public static void Put(this NetDataWriter writer, Vector3 value) {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
        }
        public static void Put(this NetDataWriter writer, Quaternion value)
        {
            writer.Put(value.x);
            writer.Put(value.y);
            writer.Put(value.z);
            writer.Put(value.w);
        }
        public static Vector3 GetVector3(this NetDataReader reader)
        {
            return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        public static Quaternion GetQuaternion(this NetDataReader reader)
        {
            return new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        internal static void AddRpcMessage(this Message message, int netId, string rpc, object[] objects)
        {
            message.Add(netId);
            message.Add(rpc);
            if (objects != null && objects.Length > 0)
            {
                message.Add(true);
                message.Add(JsonConvert.SerializeObject(objects));
            }
            else
            {
                message.Add(false);
            }
        }
    }
}
