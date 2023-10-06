using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item")]
public class ItemSO : ScriptableObject
{
    public Item item;
}

[System.Serializable]
public class Item
{
    public string itemName;
    public GameObject itemPrefab;
}
