using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviourPunCallbacks
{
    public Action<GameObject> OnInteractAction;
    [SerializeField]protected UnityEvent<GameObject> OnInteract;
    private bool interacting;

    [Space]
    [Header("FillInteraction")]
    [SerializeField] private bool fillInteraction;
    [SerializeField, Range(0f,2f)] private float fillSpeed = 1f;
    [SerializeField, Range(0f,1f)] private float fillLossSpeed = 1f;
    private float fillCurrent = 0f;
    private bool fillInteractionRunned;

    public virtual void Start()
    {
        OnInteractAction += (GameObject whoInteracted) => OnInteract?.Invoke(whoInteracted);
    }

    private void Update()
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

        interacting = true;
        OnInteractAction?.Invoke(whoInteracted);
    }

    public virtual void InteractHold(GameObject whoInteracted)
    {
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
