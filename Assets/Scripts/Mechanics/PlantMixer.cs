using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlantMixer : MonoBehaviourPun
{
    private bool plantMixerWasUsed = false;
    [SerializeField] private Transform plantDisplacement;
    [SerializeField] private ItemSO acidPlant;

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
        if (plantMixerWasUsed) return;

        whoInteracted.TryGetComponent(out PlayerInventory pInventory);

        if (pInventory == null) return;

        Item item = pInventory.GetSelectedItem();

        if(item != null && item.specialUse == SpecialUseItem.PLANT)
        {
            pInventory.RemoveItemOnHand();
            photonView.RPC(nameof(RPC_AddPlant), RpcTarget.All, item.itemName);
        }
    }

    [PunRPC]
    private void RPC_AddPlant(string plantName)
    {
        //Debug.Log(plantName);
        plantsInside.Add(Resources.Load<ItemSO>("Item/Plantas/" + plantName).item);

        UpdatePlantLights();

        if (PhotonNetwork.IsMasterClient) CheckPlants();
    }

    private void CheckPlants()
    {
        if (plantsInside.Count >= 3) 
        {
            bool plantRecipeIsCorrect = PlantMixerRecipes.Instance.CheckPlantRecipe(plantsInside.ToArray());
            plantsInside.Clear();
            photonView.RPC(nameof(RPC_CheckPlants), RpcTarget.All, plantRecipeIsCorrect);
        }
    }

    [PunRPC]
    private void RPC_CheckPlants(bool plantRecipeIsCorrect)
    {
        if (plantRecipeIsCorrect)
        {
            if(PhotonNetwork.IsMasterClient) DropAcid();
            PlaySound(successSound);

            plantMixerWasUsed = true;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient) AuditionTrigger.InstantiateAuditionTrigger(transform.position, 100f, .1f);
            PlaySound(failedSound);

            UpdatePlantLights();
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

    private void DropAcid()
    {
        PhotonNetwork.Instantiate("Prefabs/" + acidPlant.item.itemPrefab.name, plantDisplacement.position, Quaternion.identity);
    }

   
}
