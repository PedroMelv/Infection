using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : Interactable
{
    [Space]
    [SerializeField]private ItemSO item;

    protected override void Start()
    {
        OnInteractAction += GrabItem;
    }

    protected virtual void GrabItem(GameObject whoInteracted)
    {
        PlayerInventory pInventory = whoInteracted.gameObject.GetComponent<PlayerInventory>();

        if (pInventory != null)
        {
            pInventory.AddItem(item.item);
            CallDestroy();
        }
    }

    protected void CallDestroy()
    {
        photonView.RPC(nameof(RPC_DestroyMe), RpcTarget.All);
    }

    

}
