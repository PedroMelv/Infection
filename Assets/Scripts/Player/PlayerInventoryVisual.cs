using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryVisual : MonoBehaviourPun
{

    [SerializeField]private Transform[] hands;
    private Vector3[] handPos;
    private List<GameObject> visualItens = new List<GameObject>();

    private PlayerInventory pInventory;

    private void Awake()
    {
        pInventory = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        if (pInventory != null)
        {
            pInventory.OnAddItem += (bool added, Item itemAdded) => { if (added) UpdateVisual(); };
            pInventory.OnItemSlotChange += (int curSlot) => UpdateVisual();
            pInventory.OnDropItem += () => UpdateVisual();
        }
        else
        {
            Debug.Log("Não tem inventário");
        }

        UpdateVisual();
    }

    public void SetHands(Transform[] hands)
    {
        this.hands = hands;
        handPos = new Vector3[2];
        for (int i = 0; i < hands.Length; i++)
        {
            handPos[i] = hands[i].localPosition;
        }
    }


    private void UpdateVisual()
    {
        InventoryData data = pInventory.GetInventory();

        for (int i = 0; i < hands.Length; i++)
        {
            int opositeHand = (data.curSlot == i) ? 1 : 0;
            hands[i].LeanMoveLocal(handPos[opositeHand],.05f);

            foreach (Transform child in hands[i])
            {
                Destroy(child.gameObject);
            }
        }


        for (int i = 0; i < data.slots.Length; i++)
        {
            if (data.slots[i] != null)
            {
                GameObject itemHandPrefab = data.slots[i].itemHandPrefab;

                
                if (itemHandPrefab != null)
                {
                    GameObject newItem = Instantiate(itemHandPrefab, hands[i].position, hands[i].rotation);
                    newItem.transform.SetParent(hands[i]);
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
    }
}
