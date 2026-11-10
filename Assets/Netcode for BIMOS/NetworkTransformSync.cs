using LiteNetLib.Utils;
using Riptide;
using System;
using UnityEngine;

namespace BIMOS.Netcode
{
    public class NetworkTransformSync : MonoBehaviour, INetworked
    {
        public bool syncScale;
        public bool physicsSync;

        private Rigidbody body;
        private Vector3 netPos;
        private Quaternion netRot;    
        private Vector3 velocity;
        void Start()
        {
            if(physicsSync) body = GetComponent<Rigidbody>();
        }

        public void OnSerialize(NetDataWriter stream)
        {
            if (physicsSync)
            {
                stream.Put(body.position);
                stream.Put(body.rotation);
                stream.Put(body.linearVelocity);
                stream.Put(body.angularVelocity);
            }
            else
            {
                stream.Put(transform.position);
                stream.Put(transform.rotation);
            }
            if (syncScale)
            {
                stream.Put(transform.localScale);
            }       
            stream.Put(NetworkClient.ServerTick);
        }
        public void OnDeserialize(NetDataReader stream)
        {
            netPos = stream.GetVector3();
            netRot = stream.GetQuaternion();
            if (physicsSync)
            {
                body.linearVelocity = stream.GetVector3();
                body.angularVelocity = stream.GetVector3();
            }
            if (syncScale)
            {
                transform.localScale=stream.GetVector3();
            }
        }
    }
}
