using LiteNetLib.Utils;
using Riptide;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace BIMOS.Netcode
{
    public class NetworkedInteractable : NetworkBehaviour, IGrabbable, IInteractable
    {
        public UnityEvent
             TriggerDownEvent,
             TriggerUpEvent,
             PrimaryDownEvent,
             PrimaryUpEvent,
             SecondaryDownEvent,
             SecondaryUpEvent,
             GrabEvent,
             ReleaseEvent;
        public TickEvent OnTick;
        public TickEvent OnPhysicsTick;
        public bool AllowDesync = true;
        public bool AutoOwnership = true;
        private Grab _grab;

        private byte CMD_GRAB;
        private byte RPC_GRAB;
        private byte CMD_RELEASE;
        private byte RPC_RELEASE;
        private void Awake()
        {
            _grab = GetComponent<Grab>();
            _grab.ReleaseEvent += OnRelease;
        }
        public override void RegisterRpcs(NetworkIdentity identity)
        {
            CMD_GRAB = identity.RegisterCommand(CmdGrab);
            RPC_GRAB = identity.RegisterRpc((_)=>RpcGrab());
            CMD_RELEASE = identity.RegisterCommand(CmdRelease);
            RPC_RELEASE = identity.RegisterRpc((_)=>RpcRelease());
        }

        private void RpcRelease()
        {
            ReleaseEvent.Invoke();
        }

        private void CmdRelease(Connection sender, NetDataReader reader)
        {
            if (AllowDesync) identity.SendClientRpc(RPC_RELEASE, sender, MessageSendMode.Reliable);
            else identity.SendClientRpc(RPC_RELEASE, MessageSendMode.Reliable);
        }

        private void RpcGrab()
        {
            GrabEvent.Invoke();
        }

        private void CmdGrab(Connection sender, NetDataReader reader)
        {
            if (AutoOwnership) identity.SetOwnership(sender);
            if (AllowDesync) identity.SendClientRpc(RPC_GRAB, sender, MessageSendMode.Reliable);
            else identity.SendClientRpc(RPC_GRAB, MessageSendMode.Reliable);
        }

        private void CheckInputs(out float trigger, out bool primary, out bool secondary)
        {
            float leftTrigger = 0f;
            bool leftPrimary = false;
            bool leftSecondary = false;
            if (_grab.LeftHand)
            {
                leftTrigger = _grab.LeftHand.HandInputReader.Trigger;
                leftPrimary = _grab.LeftHand.HandInputReader.PrimaryButton;
                leftSecondary = _grab.LeftHand.HandInputReader.SecondaryButton;
            }

            float rightTrigger = 0f;
            bool rightPrimary = false;
            bool rightSecondary = false;
            if (_grab.RightHand)
            {
                rightTrigger = _grab.RightHand.HandInputReader.Trigger;
                rightPrimary = _grab.RightHand.HandInputReader.PrimaryButton;
                rightSecondary = _grab.RightHand.HandInputReader.SecondaryButton;
            }

            trigger = Mathf.Max(leftTrigger, rightTrigger);
            primary = leftPrimary || rightPrimary;
            secondary = leftSecondary || rightSecondary;
        }
        public void Tick()
        {
            float trigger;
            bool primary, secondary;
            CheckInputs(out trigger, out primary, out secondary);
            OnTick.Invoke(trigger, primary, secondary);
        }
        public void PhysicsTick()
        {
            float trigger;
            bool primary, secondary;
            CheckInputs(out trigger, out primary, out secondary);
            OnPhysicsTick.Invoke(trigger, primary, secondary);
        }
        public void OnTrigger(bool isButtonDown)
        {
            if (isButtonDown)
                TriggerDownEvent.Invoke();
            else
                TriggerUpEvent.Invoke();
        }
        public void OnPrimary(bool isButtonDown)
        {
            if (isButtonDown)
                PrimaryDownEvent.Invoke();
            else
                PrimaryUpEvent.Invoke();
        }
        public void OnSecondary(bool isButtonDown)
        {
            if (isButtonDown)
                SecondaryDownEvent.Invoke();
            else
                SecondaryUpEvent.Invoke();
        }
        public void OnGrab()
        {
            if(AllowDesync) RpcGrab();
            identity.SendCommand(CMD_GRAB, MessageSendMode.Reliable);
        }
        public void OnRelease()
        {
            if (AllowDesync) RpcRelease();
            identity.SendCommand(CMD_RELEASE, MessageSendMode.Reliable);
        }

        [System.Serializable]
        public class TickEvent : UnityEvent<float, bool, bool> { }
    }
}
