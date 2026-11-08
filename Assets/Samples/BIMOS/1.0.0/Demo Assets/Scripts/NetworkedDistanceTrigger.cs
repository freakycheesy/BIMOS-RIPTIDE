using BIMOS.Netcode;
using Riptide;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedDistanceTrigger : NetworkBehaviour, IDistanceTrigger
{
    public UnityEvent OnActivate, OnDeactivate;
    public UpdateEvent OnUpdate;
    public bool AllowDesync = true;

    public float Value;

    [SerializeField]
    private float _threshold = 0.05f, _maxDistance = 0.5f;

    [SerializeField]
    private Transform _activatedTransform;

    private bool _isActivated;

    // RPCs
    private byte CMD_ACTIVATE;
    private byte RPC_ACTIVATE;
    private byte CMD_DEACTIVATE;
    private byte RPC_DEACTIVATE;

    public override void RegisterRpcs(NetworkIdentity identity)
    {
        CMD_ACTIVATE=identity.RegisterCommand(CmdActivate);
        RPC_ACTIVATE = identity.RegisterRpc((_) => RpcActivated());
        CMD_DEACTIVATE = identity.RegisterCommand(CmdDeactivate);
        RPC_DEACTIVATE = identity.RegisterRpc((_) => RpcDeactivated());
    }

    private void CmdDeactivate(Connection sender, Message reader)
    {
        if (AllowDesync)
        {
            identity.SendClientRpc(RPC_DEACTIVATE, sender);
        }
        else
        {
            identity.SendClientRpc(RPC_DEACTIVATE);
        }
    }

    private void CmdActivate(Connection sender, Message reader)
    {
        if (AllowDesync)
        {
            identity.SendClientRpc(RPC_ACTIVATE, sender);
        }
        else
        {
            identity.SendClientRpc(RPC_ACTIVATE);
        }
    }

    private void FixedUpdate()
    {
        Value = GetValue();
        OnUpdate.Invoke(Value);

        if (!_isActivated && Value + _threshold >= 1)
            Activated();

        if (_isActivated && Value - _threshold <= 0)
            Deactivated();
    }

    private float GetValue()
    {
        return 1 - Mathf.Clamp01(Vector3.Distance(transform.position, _activatedTransform.position) / _maxDistance);
    }
    public void Activated()
    {
        if (AllowDesync) RpcActivated();
        identity.SendCommand(CMD_ACTIVATE);
    }

    public void Deactivated()
    {
        if (AllowDesync) RpcDeactivated();
        identity.SendCommand(CMD_DEACTIVATE);
    }
    private void RpcActivated()
    {
        _isActivated = true;
        OnActivate.Invoke();
    }

    private void RpcDeactivated()
    {
        _isActivated = false;
        OnDeactivate.Invoke();
    }

    [System.Serializable]
    public class UpdateEvent : UnityEvent<float> { }
}