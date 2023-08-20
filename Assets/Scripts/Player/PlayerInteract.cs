using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask interactLayer;

    private PlayerInput pInput;

    private Ray mouseRay;

    private void Start()
    {
        InitializeInteractions();
    }

    private void Update()
    {
        Debug.DrawRay(mouseRay.origin, mouseRay.direction * interactRange, Color.red);
    }

    private void InitializeInteractions()
    {
        pInput = GetComponent<PlayerInput>();
        if (pInput != null) 
        {
            pInput.OnInteractPress += () => HandleInteraction();
        }
    }
    private void HandleInteraction()
    {
        mouseRay = pInput.myCamera.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(mouseRay, out RaycastHit hit, interactRange, interactLayer))
        {
            Interactable detectedInteractable = hit.collider.GetComponent<Interactable>();

            if(detectedInteractable != null) 
            {
                detectedInteractable.Interact(this.gameObject);
            }
        }
    }
}
