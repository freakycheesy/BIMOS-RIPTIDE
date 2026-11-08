using LiteNetLib.Utils;
using Riptide;
using UnityEngine;

namespace BIMOS.Netcode
{
    public delegate void CommandCallback(Connection sender, NetDataReader reader);
}
