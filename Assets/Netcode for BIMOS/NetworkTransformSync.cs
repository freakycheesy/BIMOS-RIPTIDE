using Riptide;
using System;
using UnityEngine;

namespace BIMOS.Netcode
{
    public class NetworkTransformSync : NetworkBehaviour
    {
        public bool syncScale;
        public bool physicsSync;

        private Rigidbody body;
        private Vector3 netPos;
        private Quaternion netRot;    
        private Vector3 velocity;
        public override void OnStart()
        {
            if(physicsSync) body = GetComponent<Rigidbody>();
        }
        private void FixedUpdate()
        {
            if (identity.isMine)
            {
                identity.Serialize();
            }
        }
        public override void OnSerialize(Message stream)
        {
            if (physicsSync)
            {
                stream.Add(body.position);
                stream.Add(body.rotation);
                stream.Add(body.linearVelocity);
                stream.Add(body.angularVelocity);
            }
            else
            {
                stream.Add(transform.position);
                stream.Add(transform.rotation);
            }
            if (syncScale)
            {
                stream.Add(transform.localScale);
            }       
            stream.Add(Environment.TickCount);
        }
        public override void OnDeserialize(Message stream)
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
