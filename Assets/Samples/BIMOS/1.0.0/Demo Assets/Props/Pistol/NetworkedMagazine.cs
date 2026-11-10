using BIMOS.Netcode;
using LiteNetLib.Utils;
using UnityEngine;

public class NetworkedMagazine : MonoBehaviour, INetworked, IMagazine
{
    public int MaxRounds;
    public int RemainingRounds;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private GameObject[] _visualRounds;

    private void Awake()
    {
        UpdateAnimator();

        for (int i = _visualRounds.Length - 1; i > 0; i--)
            if (RemainingRounds <= i)
                _visualRounds[i]?.SetActive(false);
    }

    public void RemoveRound()
    {
        RemainingRounds--;
        _visualRounds[RemainingRounds]?.SetActive(false);
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        _animator.SetFloat("Full", (float)RemainingRounds / MaxRounds);
    }

    public void OnSerialize(NetDataWriter stream)
    {
        stream.Put(RemainingRounds);
    }

    public void OnDeserialize(NetDataReader stream)
    {
        RemainingRounds = stream.GetInt();
    }
}
