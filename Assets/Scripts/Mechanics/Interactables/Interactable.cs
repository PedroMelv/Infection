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

[RequireComponent(typeof(PhotonView))]
public class Interactable : MonoBehaviourPunCallbacks
{
    public Action<GameObject> OnInteractAction;
    [SerializeField]protected UnityEvent<GameObject> OnInteract;
    protected bool interacting;

    protected PlayerInput lastInteract;

    [SerializeField] protected CharacterInteract characterToInteract;

    public bool canInteract = true;

    [Space]
    [Header("Item Interaction")]
    public string interactionName;
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
        if (!canInteract) return;

        if(!interacting && fillCurrent > 0f)
        {
            if(fillLossSpeed == 0) 
            {
                fillCurrent = 0f;
                fillInteractionRunned = false;
                return;
            }
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
        if (!canInteract) return;
        if (fillInteraction) return;

        int curPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties["c"];
        if (characterToInteract != CharacterInteract.ANYONE && curPlayer != (int)characterToInteract) return;
        
        PlayerInput pInput = whoInteracted.GetComponent<PlayerInput>();

        if (needItem)
        {
            PlayerInventory pInventory = whoInteracted.GetComponent<PlayerInventory>();
            

            if (pInventory == null) return;
            
            Item curItem = pInventory.GetSelectedItem();
            if (curItem != itemToInteract.item) return;

            

            if (removeItem)
            {
                pInventory.DestroyItem();
                if(andAddAnother)
                {
                    pInventory.AddItem(itemToAdd.item);
                }
            }
        }

        lastInteract = pInput;
        interacting = true;
        OnInteractAction?.Invoke(whoInteracted);
    }

    public virtual float InteractHold(GameObject whoInteracted)
    {
        if (!canInteract) return 0f;
        //TODO: adicionar op��o de needItem
        if (needItem) return 0f;
        if (!fillInteraction) return 0f;

        int curPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties["c"];
        if (characterToInteract != CharacterInteract.ANYONE && curPlayer != (int)characterToInteract) return 0f;

        interacting = true;

        
        if (fillCurrent >= 1f)
        {
            //HandleInteraction
            if(!fillInteractionRunned)
            {
                OnInteractAction?.Invoke(whoInteracted);
                fillInteractionRunned = true;
            }
            else return 0f;
        }
        else
        {
            fillCurrent += Time.deltaTime * fillSpeed;
        }

        return fillCurrent;
    }

    public virtual void InteractRelease(GameObject whoInteracted)
    {
        if (!canInteract) return;
        interacting = false;
        
        if (!fillInteraction) return;
        fillInteractionRunned = false;
    }

    public bool CanInteract(out string whoCanInteractName)
    {
        int curPlayer = (int)PhotonNetwork.LocalPlayer.CustomProperties["c"];

        whoCanInteractName = ((int)characterToInteract == 1) ? "Chris" : "Penny"; 

        if ((int)characterToInteract == 0) return true;
        return curPlayer == (int)characterToInteract;
    }

    public bool NeedItem(out string itemName)
    {
        if (itemToInteract == null) itemName = ""; else itemName = itemToInteract.item.itemName;
        return itemToInteract != null;
    }

    [PunRPC]
    public void RPC_DestroyMe()
    {
        Destroy(this.gameObject);
    }

    [PunRPC]
    public void RPC_InteractDrawer()
    {
        (this as DrawerInteractable).InteractDrawer();
    }
}
