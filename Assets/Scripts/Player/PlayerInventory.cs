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

    public Action<bool, Item> OnAddItem;
    public Action<int> OnItemSlotChange;
    public Action OnDropItem;

    private PlayerInput pInput;
    private PlayerHealth pHealth;


    private void Awake()
    {
        pInput = GetComponent<PlayerInput>();
        pHealth = GetComponent<PlayerHealth>();

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
        if (pInput.mouse_scroll != 0f) ChangeItemSlot();
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

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemPrefab == null)
            {
                slots[i] = addItem;
                couldAdd = true;
                break;
            }
        }

        OnAddItem?.Invoke(couldAdd, addItem);
        return couldAdd;
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

    public void DropItem(bool useForward = true)
    {
        DropItem(ref slots[itemSlotSelected], useForward);
    }
    public void DropItem(ref Item slot, bool useForward = true)
    {
        if (slot != null && slot.itemPrefab != null && slot.canDrop) 
        {
            Vector3 dir = (useForward) ? pInput.myCamera.transform.forward : Vector3.up;

            float dstToTarget = Vector3.Distance(transform.position, transform.position + dir * 1.25f);
            bool hit = Physics.Raycast(transform.position, dir * 1.25f, dstToTarget);

            Debug.Log("Hitted: " + hit);

            if (!hit)
            {
                PhotonNetwork.Instantiate("Prefabs/" + slot.itemPrefab.name, transform.position + dir * 1.25f, Quaternion.identity);

                slot = new Item();
                OnDropItem?.Invoke();
            }
        }
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

