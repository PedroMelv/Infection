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
    public bool canDrop = true;
    public bool canUseOnTrashbin = true;
    public SpecialUseItem specialUse;
    [Space]
    public GameObject itemPrefab;
    public GameObject itemHandPrefab;
    [Space]
    public float reloadTime;

    #region Overloading Operators
    
    #endregion
}

public enum SpecialUseItem
{
    NORMAL,
    GUN,
    BLOOD_SPLICER_EMPTY,
    BLOOD_SPLICER_FULL,
    PLANT
}
