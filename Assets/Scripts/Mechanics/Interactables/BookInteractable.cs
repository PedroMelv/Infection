using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookInteractable : Interactable
{
    [SerializeField] private BookUI book;

    protected override void Start()
    {
        base.Start();
        OnInteractAction += OpenBook;
    }

    private void OpenBook(GameObject whoInteracted)
    {
        Debug.Log("Opening Book");
        PlayerInput pInput = whoInteracted.GetComponent<PlayerInput>();
        book.gameObject.SetActive(true);
        book.OpenBook(pInput);
    }
}
