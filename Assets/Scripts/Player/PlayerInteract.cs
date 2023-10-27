using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerInteract : MonoBehaviourPun
{
    [SerializeField] private float interactRange;
    [SerializeField] private LayerMask interactLayer;
    private TextMeshProUGUI interactText;

    private Interactable holdingInteractable;

    private PlayerInput pInput;
    private PlayerMovement pMove;

    private Ray mouseRay;

    private void Awake()
    {
        pMove = GetComponent<PlayerMovement>();
        pInput = GetComponent<PlayerInput>();
    }
    private void Start()
    {
        interactText = GameObject.FindGameObjectWithTag("InteractText").GetComponent<TextMeshProUGUI>();
        interactText.SetText("");
        InitializeInteractions();
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;
        mouseRay = pInput.myCamera.ScreenPointToRay(Input.mousePosition);

        HandleInteractionHover();

        Debug.DrawRay(mouseRay.origin, mouseRay.direction * interactRange, Color.red);
    }

    private void InitializeInteractions()
    {
        pInput = GetComponent<PlayerInput>();
        if (pInput != null) 
        {
            pInput.OnInteractPress   += () => HandleInteractionPress();
            pInput.OnInteractHold    += () => HandleInteractionHold();
            pInput.OnInteractRelease += () => HandleInteractionRelease();
        }
    }

    private void HandleInteractionHover()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit, interactRange, interactLayer))
        {
            Interactable[] detectedInteractable = hit.collider.GetComponents<Interactable>();
            string interactString = "";

            for (int i = 0; i < detectedInteractable.Length; i++)
            {
                if (detectedInteractable[i] != null)
                {
                    if (detectedInteractable.Length == 1)
                    {
                        interactString += detectedInteractable[i].interactionName;
                    }
                    else
                    {
                        interactString += "-" + detectedInteractable[i].interactionName + ".\n";
                    }
                }
            }

            interactText.SetText(interactString);
        }
        else
        {
            interactText.SetText("");
        }
    }

    private void HandleInteractionPress()
    {
        if(Physics.Raycast(mouseRay, out RaycastHit hit, interactRange, interactLayer))
        {
            Interactable[] detectedInteractable = hit.collider.GetComponents<Interactable>();

            for (int i = 0; i < detectedInteractable.Length; i++)
            {
                if(detectedInteractable[i] != null) 
                {
                    detectedInteractable[i].Interact(this.gameObject);
                }       
            }
        }
    }
    private void HandleInteractionHold()
    {
        if (Physics.Raycast(mouseRay, out RaycastHit hit, interactRange, interactLayer))
        {
            Interactable detectedInteractable = hit.collider.GetComponent<Interactable>();

            if (detectedInteractable != null)
            {
                holdingInteractable = detectedInteractable;
                detectedInteractable.InteractHold(this.gameObject);
                interactText.SetText("Interacting...");
            }
            else
                HandleInteractionRelease();
        }
        else
            HandleInteractionRelease();
    }
    private void HandleInteractionRelease()
    {
        if (holdingInteractable != null)
        {
            holdingInteractable.InteractRelease(this.gameObject);
        }

        interactText.SetText("");
    }

    private void HandleCustomInteractions(Interactable interactable)
    {
        if(interactable is DuctTrigger)
        {
            pMove.InteractDuct((interactable as DuctTrigger));
        }
    }
}
