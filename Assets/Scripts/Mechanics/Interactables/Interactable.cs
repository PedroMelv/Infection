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

    public virtual void Start()
    {
        OnInteractAction += (GameObject whoInteracted) => OnInteract?.Invoke(whoInteracted);
    }

    public virtual void Interact(GameObject whoInteracted)
    {
        OnInteractAction?.Invoke(whoInteracted);
    }
}
