using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerInteractable : Interactable
{
    [SerializeField] private ItemSO[] itemsInside;
    private List<Interactable> itemsInsideObj = new List<Interactable>();
    private bool isOpened;

    protected override void Start()
    {
        OnInteractAction += (GameObject whoInteracted) => CallInteractDrawer();
        base.Start();

        if(PhotonNetwork.IsMasterClient)
            InitializeItems();

        ChangeItemsInteractions(false);
    }

    private void InitializeItems()
    {
        for(int i = 0; i < itemsInside.Length; i++)
        {
            ItemSO item = itemsInside[i];

            GameObject newItem = PhotonNetwork.Instantiate("Prefabs/" + item.item.itemPrefab.name, Vector3.zero, Quaternion.identity);
            newItem.transform.parent = transform;
            newItem.transform.localPosition = Vector3.up * 0.23f;
            newItem.GetComponent<Rigidbody>().isKinematic = true;
            newItem.GetComponent<Rigidbody>().useGravity = false;

            itemsInsideObj.Add(newItem.GetComponent<Interactable>());
        }
    }

    private void CallInteractDrawer()
    {
        photonView.RPC(nameof(RPC_InteractDrawer), RpcTarget.All);
    }

    public void InteractDrawer()
    {
        isOpened = !isOpened;

        ChangeItemsInteractions(isOpened);

        if (isOpened)
        {
            transform.LeanMoveLocalX(-.75f, .15f);
        }
        else
        {
            transform.LeanMoveLocalX(0f, .15f);
        }
    }

    private void ChangeItemsInteractions(bool canInteract)
    {
        for (int i = 0; i < itemsInsideObj.Count; i++)
        {
            if (itemsInsideObj[i] != null) itemsInsideObj[i].canInteract = canInteract;
        }
    }
}
