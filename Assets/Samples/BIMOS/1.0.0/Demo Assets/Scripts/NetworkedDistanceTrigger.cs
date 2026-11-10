using BIMOS.Netcode;
using LiteNetLib.Utils;
using Riptide;
using UnityEngine;
using UnityEngine.Events;

public class NetworkedDistanceTrigger : MonoBehaviour, IRpc, IDistanceTrigger
{
    private NetworkIdentity netIdentity;
    public UnityEvent OnActivate, OnDeactivate;
    public UpdateEvent OnUpdate;
    public bool AllowDesync = true;

    public float Value;

    [SerializeField]
    private float _threshold = 0.05f, _maxDistance = 0.5f;

    [SerializeField]
    private Transform _activatedTransform;

    private bool _isActivated;

    void Awake()
    {
        netIdentity = GetComponentInParent<NetworkIdentity>();
    }

    [Rpc(RpcTargets.Server)]
    private void CmdDeactivate(ushort sender)
    {
        if (AllowDesync)
        {
            netIdentity.SendClientRpc(nameof(RpcDeactivated), sender);
        }
        else
        {
            netIdentity.SendClientRpc(nameof(RpcDeactivated));
        }
    }

    [Rpc(RpcTargets.Server)]
    private void CmdActivate(ushort sender)
    {
        if (AllowDesync)
        {
            netIdentity.SendClientRpc(nameof(RpcActivated), sender);
        }
        else
        {
            netIdentity.SendClientRpc(nameof(RpcActivated));
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
        netIdentity.SendServerRpc(nameof(CmdActivate));
    }

    public void Deactivated()
    {
        if (AllowDesync) RpcDeactivated();
        netIdentity.SendServerRpc(nameof(CmdDeactivate));
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