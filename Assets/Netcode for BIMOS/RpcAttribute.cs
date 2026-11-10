using System;

namespace BIMOS.Netcode
{
    public class RpcAttribute : Attribute
    {
        public RpcTargets Type { get; private set; }
        public RpcAttribute(RpcTargets type)
        {
            Type = type;
        }
    }
}
