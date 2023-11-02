using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMixer : MonoBehaviour
{
    private List<Item> plantsInside = new List<Item>();


    private void Start()
    {
        GetComponent<Interactable>().OnInteractAction += AddPlant;    
    }

    private void AddPlant(GameObject whoInteracted)
    {
        whoInteracted.TryGetComponent(out PlayerInventory pInventory);

        if (pInventory == null) return;

        Item item = pInventory.GetSelectedItem();

        if(item != null && item.specialUse == SpecialUseItem.PLANT)
        {
            plantsInside.Add(item);
            pInventory.RemoveItemOnHand();

            CheckPlants();
        }
    }

    private void CheckPlants()
    {
        if(plantsInside.Count >= 3) 
        {
            Debug.Log("Plant is: " + PlantMixerRecipes.Instance.CheckPlantRecipe(plantsInside.ToArray()));
            plantsInside.Clear();
        }
    }
}
