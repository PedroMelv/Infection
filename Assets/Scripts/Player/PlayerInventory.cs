using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private int itemSlotSelected = 0;
    private int slotsAmount = 2;

    [SerializeField] private ItemSO gunItem;
    [SerializeField] private Item[] slots;

    [SerializeField] private LayerMask dropLayerMask;

    public Action<bool, Item> OnAddItem;
    public Action<int> OnItemSlotChange;
    public Action OnDropItem;

    private PlayerInput pInput;
    private PlayerHealth pHealth;
    private PlayerCombat pCombat;


    private void Awake()
    {
        pInput  = GetComponent<PlayerInput>();
        pHealth = GetComponent<PlayerHealth>();
        pCombat = GetComponent<PlayerCombat>();

        slots = new Item[slotsAmount];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new Item();
        }
    }
    private void Start()
    {
        pInput.OnTabPressed += () => ChangeItemSlot();

        bool hasWeapon = ((int)PhotonNetwork.LocalPlayer.CustomProperties["c"] == 1);

        if(hasWeapon)
        {
            AddItem(gunItem.item);
        }

        pHealth.OnDie += () => DropItems();
    }
    private void Update()
    {
        if (pInput.dropInputPressed) DropItem();
        if (pInput.mouse_scroll != 0f && !pCombat.IsReloading()) ChangeItemSlot();
    }

    public void ChangeItemSlot()
    {
        itemSlotSelected++;
        if (itemSlotSelected >= slotsAmount) itemSlotSelected = 0;

        OnItemSlotChange?.Invoke(itemSlotSelected);
    }

    public bool AddItem(Item addItem)
    {
        bool couldAdd = false;

        if (slots[itemSlotSelected] != null && slots[itemSlotSelected].itemPrefab == null)
        {
            slots[itemSlotSelected] = addItem;
            couldAdd = true;
        }
        else
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].itemPrefab == null)
                {
                    slots[i] = addItem;
                    couldAdd = true;
                    break;
                }
            }
        }


        OnAddItem?.Invoke(couldAdd, addItem);
        return couldAdd;
    }
    
    public void RemoveItemOnHand()
    {
        DestroyItem(ref slots[itemSlotSelected]);
    }
    public void DestroyItem()
    {
        DestroyItem(ref slots[itemSlotSelected]);
    }
    public void DestroyItem(ref Item slot)
    {
        if (slot != null && slot.itemPrefab != null)
        {
            slot = new Item();
            OnDropItem?.Invoke();
        }
    }

    public GameObject DropItem(bool useForward = true)
    {
        return DropItem(ref slots[itemSlotSelected], useForward);
    }
    public GameObject DropItem(ref Item slot, bool useForward = true)
    {
        if (slot != null && slot.itemPrefab != null && slot.canDrop) 
        {
            Transform cameraTransform = pInput.cameraPos.transform;
            Vector3 playerForward = pInput.playerLookingDir.forward;
            
            Vector3 dir = (useForward) ? playerForward : Vector3.up;


            float dstToTarget = Vector3.Distance(cameraTransform.position, cameraTransform.position + dir * .5f);
            bool hittedAnything = Physics.Raycast(cameraTransform.position, dir, out RaycastHit hit, dstToTarget, dropLayerMask);
            Debug.DrawLine(cameraTransform.position, cameraTransform.position + dir, Color.green, 300f);

            Vector3 spawnPos = cameraTransform.position + dir;

            if (hittedAnything)
            {
                Debug.Log("Hitted: " + spawnPos);
                return null;
            }
            
            int itemIndex = ItemsManager.Instance.GetItemIndex(slot);

            if (itemIndex == -1) { Debug.Log("Item n�o existe na database"); return null; }

            GameObject droppedItem = PhotonNetwork.Instantiate("Prefabs/" + slot.itemPrefab.name, spawnPos, Quaternion.identity);

            droppedItem.GetComponent<ItemInteractable>().SetItem(itemIndex);
            droppedItem.GetComponent<Rigidbody>().AddForce((Vector3.up * .35f + playerForward) * 5.25f, ForceMode.VelocityChange);

            slot = new Item();
            OnDropItem?.Invoke();

            ChangeItemSlot();

            return droppedItem;
        }
        return null;
    }

    public void DropItems()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            DropItem(ref slots[i], false);
        }
    }

    public InventoryData GetInventory()
    {
        InventoryData data = new InventoryData(slots, itemSlotSelected);
        return data;
    }
    public Item GetSelectedItem()
    {
        return slots[itemSlotSelected];
    }
}

public struct InventoryData
{
    public Item[] slots;
    public int curSlot;

    public InventoryData(Item[] slots, int curSlot)
    {
        this.slots = slots;
        this.curSlot = curSlot;
    }
}

