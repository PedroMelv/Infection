using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantMixer : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] mixerLights;
    [SerializeField] private Material redLightMat, greenLightMat;
    [SerializeField] private AudioClip failedSound, successSound;
    private AudioSource aSource;

    private List<Item> plantsInside = new List<Item>();

    private void Awake()
    {
        aSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        GetComponent<Interactable>().OnInteractAction += AddPlant;
        UpdatePlantLights();
    }

    private void AddPlant(GameObject whoInteracted)
    {
        whoInteracted.TryGetComponent(out PlayerInventory pInventory);

        if (pInventory == null) return;

        Item item = pInventory.GetSelectedItem();

        if(item != null && item.specialUse == SpecialUseItem.PLANT)
        {
            Debug.Log("Plant: " + item.itemName);
            plantsInside.Add(item);
            pInventory.RemoveItemOnHand();

            CheckPlants();
        }
    }

    private void CheckPlants()
    {
        UpdatePlantLights();

        if (plantsInside.Count >= 3) 
        {
            bool plantRecipeIsCorrect = PlantMixerRecipes.Instance.CheckPlantRecipe(plantsInside.ToArray());
            plantsInside.Clear();

            if (plantRecipeIsCorrect)
            {
                PlaySound(successSound);
            }
            else
            {
                AuditionTrigger.InstantiateAuditionTrigger(transform.position, 100f, .1f);
                PlaySound(failedSound);

                UpdatePlantLights();
            }
        }
    }

    private void UpdatePlantLights()
    {
        for (int i = 0; i < mixerLights.Length; i++)
        {
            mixerLights[i].materials[1].color = (i < plantsInside.Count) ? greenLightMat.color : redLightMat.color;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        aSource.clip = clip;
        aSource.Play();
    }
}
