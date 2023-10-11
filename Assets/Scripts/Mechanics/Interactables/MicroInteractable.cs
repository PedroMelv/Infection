using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroInteractable : Interactable
{
    public bool minigameWasTriggered;

    private void ShowMinigame(GameObject whoInteracted)
    {
        CellArea.i.StartMinigame(whoInteracted, this);
    }

    public override void Interact(GameObject whoInteracted)
    {
        if(minigameWasTriggered)
        {
            interacting = true;

            ShowMinigame(whoInteracted);
            return;
        }
        
        int curPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties["c"];
        if (characterToInteract != CharacterInteract.ANYONE && curPlayer != (int)characterToInteract) return;

        if (needItem)
        {
            PlayerInventory pInventory = whoInteracted.GetComponent<PlayerInventory>();

            if (pInventory != null)
            {
                Item curItem = pInventory.GetSelectedItem();

                if (curItem == itemToInteract.item)
                {
                    if (removeItem)
                    {
                        pInventory.DestroyItem();
                        if (andAddAnother)
                        {
                            pInventory.AddItem(itemToAdd.item);
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }

        interacting = true;
        ShowMinigame(whoInteracted);
        minigameWasTriggered = true;
    }
}
