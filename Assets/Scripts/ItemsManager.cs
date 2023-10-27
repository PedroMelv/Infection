using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemsManager : MonoBehaviour
{
    public ItemSO[] items;

    public static ItemsManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        items = Resources.LoadAll<ItemSO>("Item");
    }


    public int GetItemIndex(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].item == item)
            {
                return i;
            }
        }
        return -1;
    }
}
