using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public enum CharacterInteract
{
    ANYONE = 0,
    CHRIS = 1,
    PENNY = 2
}

public class Interactable : MonoBehaviourPunCallbacks
{
    public Action<GameObject> OnInteractAction;
    [SerializeField]protected UnityEvent<GameObject> OnInteract;
    protected bool interacting;

    [SerializeField] protected CharacterInteract characterToInteract;

    [Space]
    [Header("Item Interaction")]
    [SerializeField] protected bool needItem;
    [SerializeField] protected ItemSO itemToInteract;
    [SerializeField] protected bool removeItem;
    [SerializeField] protected bool andAddAnother;
    [SerializeField] protected ItemSO itemToAdd;

    [Space]
    [Header("FillInteraction")]
    [SerializeField] protected bool fillInteraction;
    [SerializeField, Range(0f,2f)] protected float fillSpeed = 1f;
    [SerializeField, Range(0f,1f)] protected float fillLossSpeed = 1f;
    protected float fillCurrent = 0f;
    protected bool fillInteractionRunned;

    protected virtual void Start()
    {
        OnInteractAction += (GameObject whoInteracted) => OnInteract?.Invoke(whoInteracted);
    }

    protected virtual void Update()
    {
        if(!interacting && fillCurrent > 0f)
        {
            fillCurrent -= Time.deltaTime * fillLossSpeed;
        }
        else if(fillCurrent < 0f)
        {
            fillCurrent = 0f;
            fillInteractionRunned = false;
        }
    }

    public virtual void Interact(GameObject whoInteracted)
    {
        if (fillInteraction) return;

        int curPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties["c"];
        if (characterToInteract != CharacterInteract.ANYONE && curPlayer != (int)characterToInteract) return;

        if(needItem)
        {
            PlayerInventory pInventory = whoInteracted.GetComponent<PlayerInventory>();

            if(pInventory != null)
            {
                Item curItem = pInventory.GetSelectedItem();
            
                if(curItem == itemToInteract.item)
                {
                    if(removeItem)
                    {
                        pInventory.DestroyItem();
                        if(andAddAnother)
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
        OnInteractAction?.Invoke(whoInteracted);
    }

    public virtual void InteractHold(GameObject whoInteracted)
    {
        //TODO: adicionar opção de needItem
        if (needItem) return;

        int curPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties["c"];
        if (characterToInteract != CharacterInteract.ANYONE && curPlayer != (int)characterToInteract) return;

        if (!fillInteraction) return;

        interacting = true;

        if (fillCurrent >= 1f)
        {
            //HandleInteraction
            if(!fillInteractionRunned)
            {
                OnInteractAction?.Invoke(whoInteracted);
                fillInteractionRunned = true;
            }
        }
        else
        {
            fillCurrent += Time.deltaTime * fillSpeed;
        }
    }

    public virtual void InteractRelease(GameObject whoInteracted)
    {
        interacting = false;
        if (fillInteraction) return;
        fillInteractionRunned = false;
    }
}
