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
        OnInteractAction += (GameObject whoInteracted) => 
        {
            PlayerInventory pInventory = whoInteracted.gameObject.GetComponent<PlayerInventory>();

            if (pInventory != null)
            {
                pInventory.AddItem(item.item);
                photonView.RPC(nameof(RPC_Destroy), RpcTarget.All);
            }
        };
    }

    [PunRPC]
    private void RPC_Destroy()
    {
        Destroy(this.gameObject);
    }
}
