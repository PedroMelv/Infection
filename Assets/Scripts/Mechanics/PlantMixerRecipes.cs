using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlantMixerRecipes : MonoBehaviourPun
{
    [System.Serializable]
    private struct PlantRecipe
    {
        public bool fakeRecipe;
        public Item[] plants;

        public PlantRecipe(Item[] plants, bool fakeRecipe)
        {
            this.fakeRecipe = fakeRecipe;
            this.plants = plants;
        }
    }

    [SerializeField]private List<PlantRecipe> plantRecipes = new List<PlantRecipe>();

    [SerializeField]private ItemSO[] plantsCure;
    [SerializeField]private ItemSO[] plantsSolvent;
    [SerializeField]private ItemSO[] plantsAcid;


    public static PlantMixerRecipes Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            plantRecipes.Add(CreateFakeRecipe());
        }

        plantRecipes.Add(CreateRecipe());
    }

    private PlantRecipe CreateFakeRecipe()
    {
        List<Item> plants = new List<Item>();
        int perfectness = 0;
        bool containsCure    = false;
        bool containsSolvent = false;
        bool containsAcid    = false;

        while(plants.Count < 3)
        {
            int randomPlant = Random.Range(0, 3);

            switch (randomPlant)
            {
                case 0:
                    if (!containsCure) perfectness++;
                    containsCure = true;

                    plants.Add(plantsCure[Random.Range(0, plantsCure.Length)].item);
                break;
                case 1:
                    if (!containsSolvent) perfectness++;
                    containsSolvent = true;
                    plants.Add(plantsSolvent[Random.Range(0, plantsSolvent.Length)].item);
                    break; 
                case 2:
                    if (!containsAcid) perfectness++;
                    containsAcid = true;
                    plants.Add(plantsAcid[Random.Range(0, plantsAcid.Length)].item);
                    break;
            }
        }

        if (perfectness >= 3) return CreateFakeRecipe();
        PlantRecipe recipe = new PlantRecipe(plants.ToArray(), true);
        return recipe;
    }
    private PlantRecipe CreateRecipe()
    {
        List<Item> plants = new List<Item>
        {
            plantsCure[Random.Range(0, plantsCure.Length)].item,
            plantsSolvent[Random.Range(0, plantsSolvent.Length)].item,
            plantsAcid[Random.Range(0, plantsAcid.Length)].item
        };

        PlantRecipe recipe = new PlantRecipe(plants.ToArray(), false);
        return recipe;
    }

    public bool CheckPlantRecipe(Item[] plants)
    {
        bool found = false;
        int currentRecipe = 0;

        while(!found && currentRecipe < plantRecipes.Count)
        {
            List<Item> recipe = new List<Item>(plantRecipes[currentRecipe].plants);

            for (int i = 0; i < plants.Length; i++)
            {
                for (int j = 0; j < recipe.Count; j++)
                {
                    if (recipe[j].itemName == plants[i].itemName)
                    {
                        recipe.RemoveAt(j);
                        break;
                    }
                }
                
            }

            Debug.Log("RecipeIndex: " + currentRecipe + " recipe count: " + recipe.Count);
            if (recipe.Count == 0)
            {
                Debug.Log("Found plant");

                if (!plantRecipes[currentRecipe].fakeRecipe)
                    found = true; 
                break;
            }

            currentRecipe++;
        }

        return found;
    }
}
