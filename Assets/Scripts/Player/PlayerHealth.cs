using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Photon.Pun;
using Photon.Realtime;

public class PlayerHealth : BaseHealth
{
    [SerializeField] private GameObject interactOnDeath;
    private Volume damageVolume;

    public override void Start()
    {
        base.Start();
        damageVolume = GameObject.FindGameObjectWithTag("DamageVolume").GetComponent<Volume>();

        OnDie += () => CallOnDie();
        OnHeal += () => RemoveInteractable();
    }

    public override void Update()
    {
        base.Update();

        if(photonView.IsMine)damageVolume.weight = Mathf.Lerp(damageVolume.weight, 1 - health / maxHealth, 5f * Time.deltaTime);
    }

    private void CallOnDie()
    {
        photonView.RPC(nameof(RPC_OnDie), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_OnDie()
    {
        interactOnDeath.SetActive(true);
    }

    private void RemoveInteractable()
    {
        photonView.RPC(nameof(RPC_RemoveInteractable), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_RemoveInteractable()
    {
        interactOnDeath.SetActive(false);
    }
}
