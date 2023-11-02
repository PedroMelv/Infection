using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashbinInteractable : Interactable
{

    protected override void Start()
    {
        base.Start();
        OnInteractAction += DiscardItem;
    }

    private void DiscardItem(GameObject whoInteracted)
    {
        PlayerInventory pInventory = whoInteracted.GetComponent<PlayerInventory>();

        if(pInventory != null && pInventory.GetSelectedItem() != null && pInventory.GetSelectedItem().canUseOnTrashbin)
        {
            GameObject droppedObj = pInventory.DropItem();
            Destroy(droppedObj);
        }
    }
}
