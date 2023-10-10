using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyHealth : BaseHealth
{
    [SerializeField] private GameObject collectBloodTrigger;

    public override void Start()
    {
        base.Start();

        OnDie += () => CallChangeCollectBlood(true);
        OnHeal += () => CallChangeCollectBlood(false);
    }

    private void CallChangeCollectBlood(bool to)
    {
        photonView.RPC(nameof(RPC_ChangeCollectBlood), RpcTarget.All, to);
    }

    [PunRPC]
    private void RPC_ChangeCollectBlood(bool to)
    {
        collectBloodTrigger.SetActive(to);
    }
}
