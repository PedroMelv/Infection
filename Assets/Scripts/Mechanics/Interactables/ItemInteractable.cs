using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : Interactable
{
    [Space]
    [SerializeField] private bool destroyOnUse = true;
    [SerializeField] private ItemSO itemScriptableObject;

    protected override void Start()
    {
        base.Start();
        OnInteractAction += GrabItem;
    }

    protected virtual void GrabItem(GameObject whoInteracted)
    {
        PlayerInventory pInventory = whoInteracted.gameObject.GetComponent<PlayerInventory>();

        if (pInventory != null)
        {
            if(pInventory.AddItem(itemScriptableObject.item))
                if(destroyOnUse) CallDestroy();
        }
    }

    protected void CallDestroy()
    {
        photonView.RPC(nameof(RPC_DestroyMe), RpcTarget.All);
    }

    
    public void SetItem(int itemIndex)
    {
        if(PhotonNetwork.InRoom)photonView.RPC(nameof(RPC_SetItem), RpcTarget.All,itemIndex); 
        else
            itemScriptableObject = ItemsManager.Instance.items[itemIndex];
    }

    [PunRPC]
    public void RPC_SetItem(int itemIndex)
    {
        itemScriptableObject = ItemsManager.Instance.items[itemIndex];
    }
}
