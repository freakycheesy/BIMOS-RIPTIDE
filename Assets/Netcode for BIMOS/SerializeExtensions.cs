using LiteNetLib.Utils;
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

        public static void Serialize(this NetDataWriter writer, object value)
        {
            switch (value)
            {
                case byte i:
                    writer.Put(i);
                    break;
                case sbyte i:
                    writer.Put(i);
                    break;
                case bool i:
                    writer.Put(i);
                    break;
                case short i:
                    writer.Put(i);
                    break;
                case ushort i:
                    writer.Put(i);
                    break;
                case int i:
                    writer.Put(i);
                    break;
                case uint i:
                    writer.Put(i);
                    break;
                case ulong i:
                    writer.Put(i);
                    break;
                case float i:
                    writer.Put(i);
                    break;
                case double i:
                    writer.Put(i);
                    break;
                case string i:
                    writer.Put(i);
                    break;
                case INetSerializable i:
                    writer.Put(i);
                    break;
                case byte[] i:
                    writer.Put(i);
                    break;
                case bool[] i:
                    writer.PutArray(i);
                    break;
                case short[] i:
                    writer.PutArray(i);
                    break;
                case ushort[] i:
                    writer.PutArray(i);
                    break;
                case int[] i:
                    writer.PutArray(i);
                    break;
                case uint[] i:
                    writer.PutArray(i);
                    break;
                case long[] i:
                    writer.PutArray(i);
                    break;
                case ulong[] i:
                    writer.PutArray(i);
                    break;
                case float[] i:
                    writer.PutArray(i);
                    break;
                case double[] i:
                    writer.PutArray(i);
                    break;
                case string[] i:
                    writer.PutArray(i);
                    break;
                default:
                    RiptideLogger.Log(Riptide.Utils.LogType.Error, "Invalid type! Could not be casted!");
                    break;
            }
        }
    }
}
