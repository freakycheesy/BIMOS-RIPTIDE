using LiteNetLib.Utils;
using Riptide;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace BIMOS.Netcode
{
    public class NetworkedInteractable : MonoBehaviour, IRpc, IGrabbable, IInteractable
    {
        private NetworkIdentity netIdentity;
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
        public bool IsReliable = true;
        private Grab _grab;

        private void Awake()
        {
            netIdentity = GetComponentInParent<NetworkIdentity>();
            _grab = GetComponent<Grab>();
            _grab.ReleaseEvent += OnRelease;
        }

        [Rpc(RpcTargets.Client)]
        private void RpcRelease()
        {
            ReleaseEvent.Invoke();
        }

        [Rpc(RpcTargets.Server)]
        private void CmdRelease(ushort sender)
        {
            if (AllowDesync) netIdentity.SendClientRpc(nameof(RpcRelease), sender, IsReliable);
            else netIdentity.SendClientRpc(nameof(RpcRelease), reliable:IsReliable);
        }

        [Rpc(RpcTargets.Client)]
        private void RpcGrab()
        {
            GrabEvent.Invoke();
        }

        [Rpc(RpcTargets.Server)]
        private void CmdGrab(ushort sender)
        {
            if (AutoOwnership) netIdentity.SetOwnership(sender);
            if (AllowDesync) netIdentity.SendClientRpc(nameof(RpcGrab), sender, IsReliable);
            else netIdentity.SendClientRpc(nameof(RpcGrab), reliable:IsReliable);
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
            netIdentity.SendServerRpc(nameof(CmdGrab), reliable:IsReliable);
        }
        public void OnRelease()
        {
            if (AllowDesync) RpcRelease();
            netIdentity.SendServerRpc(nameof(CmdRelease), reliable: IsReliable);
        }

        [System.Serializable]
        public class TickEvent : UnityEvent<float, bool, bool> { }
    }
}
