using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    [SerializeField] private Transform pagesContent;
    [SerializeField] private Button leftArrow, rightArrow;
    private List<GameObject> pages = new List<GameObject>();
    private PlayerInput playerInput;
    private int currentPage;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) CloseBook();
    }

    public void OpenBook(PlayerInput pInput)
    {
        if(pages == null || pages.Count == 0)
        {
            pages = new List<GameObject>();
            foreach (Transform child in pagesContent)
            {
                pages.Add(child.gameObject);
            }
        }

        playerInput = pInput;
        playerInput.canInput = false;

        Cursor.lockState = CursorLockMode.None;

        UpdatePage();
    }

    public void CloseBook()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.canInput = true;
        gameObject.SetActive(false);
    }

    public void ChangePage(int nextPage)
    {
        currentPage = Mathf.Clamp(currentPage + nextPage, 0, pages.Count - 1);
        leftArrow.interactable = currentPage != 0;
        rightArrow.interactable = currentPage != pages.Count - 1;

        UpdatePage();
    }

    private void UpdatePage()
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(currentPage == i);
        }
    }


}
