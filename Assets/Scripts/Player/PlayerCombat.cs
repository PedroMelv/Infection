using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviourPun
{
    [SerializeField] private LayerMask hitLayer;
    [SerializeField] private GameObject bulletMark;
    [SerializeField] private AudioClip shootSound;

    private bool reloading;
    [SerializeField]private int currentAmmo;
    private int totalAmmo;
    

    private bool isAiming = false;
    private bool lastIsAiming = false;

    private GameObject ammoUI;
    private TextMeshProUGUI currentAmmoText;
    private TextMeshProUGUI totalAmmoText;

    public bool IsAiming { get { return isAiming; } private set { isAiming = value; } }

    public Action OnShoot;
    public Action<bool> IsAimingChanged;

    private PlayerInput pInput;
    private PlayerInventory pInventory;

    bool canUseWeapon = false;

    private void Awake()
    {
        pInput = GetComponent<PlayerInput>();
        pInventory = GetComponent<PlayerInventory>();
    }

    private void Start()
    {
        if (!photonView.IsMine) return;

        GetAmmoUI();
    }
    private void Update()
    {
        if (!photonView.IsMine) return;

        HandleAmmoUI();

        HandleGun();
    }

    private void HandleGun()
    {
        HandleAiming();

        if (!ammoUI.activeSelf || reloading)
        {
            isAiming = false;
            return;
        }

        IsAiming = pInput.rightMouseInput;

        HandleReload();


        if (IsAiming && pInput.leftMouseInputPressed)
        {
            if(currentAmmo == 0)
            {
                Debug.Log("No Ammo");
                return;
            }

            OnShoot?.Invoke();

            AuditionTrigger.InstantiateAuditionTrigger(transform.position, 20f, .15f);

            currentAmmo--;
            Ray mouseRay = pInput.myCamera.ScreenPointToRay(Input.mousePosition);
            bool collided = Physics.Raycast(mouseRay, out RaycastHit hit, 100f, hitLayer);

            PlayShootSound();

            if (collided)
            {
                BaseHealth hp = hit.collider.gameObject.GetComponent<BaseHealth>();

                if (hp != null)
                {
                    if (hp is EnemyHealth)
                    {
                        hp.CallTakeDamage(1, transform.position);
                        return;
                    }
                    return;
                }

                GameObject mark = PhotonNetwork.Instantiate(bulletMark.name, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                //mark.transform.position += mark.transform.forward * 0.025f;
            }
        }
    }

    private void HandleAiming()
    {
        if (lastIsAiming != isAiming)
        {
            lastIsAiming = isAiming;
            IsAimingChanged?.Invoke(isAiming);
        }
    }

    private void HandleReload()
    {
        if (pInput.reloadInputPressed)
        {
            if(totalAmmo > 0)
            {
                StartCoroutine(EReload());
            }
        }
    }

    private IEnumerator EReload()
    {
        reloading = true;
        yield return new WaitForSeconds(pInventory.GetSelectedItem().reloadTime);

        int reloadAmount = Mathf.Min(totalAmmo, 6);

        totalAmmo -= reloadAmount;
        currentAmmo += reloadAmount;

        reloading = false;
    }


    private void PlayShootSound()
    {
        photonView.RPC(nameof(RPC_PlayShootSound), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayShootSound()
    {
        AudioClip stepClip = shootSound;

        GameObject stepSound = new GameObject(stepClip.name);

        stepSound.transform.position = transform.position - Vector3.down;

        AudioSource source = stepSound.AddComponent<AudioSource>();

        source.clip = stepClip;
        source.volume = .5f;

        source.maxDistance = 20f;
        source.spatialBlend = 1f;

        source.Play();

        Destroy(stepSound, stepClip.length + .1f);
    }

    private void HandleAmmoUI()
    {

        //GetAmmoUI();

        ammoUI.SetActive((pInventory.GetSelectedItem() != null && pInventory.GetSelectedItem().specialUse == SpecialUseItem.GUN));
        
        currentAmmoText.SetText(currentAmmo.ToString("00"));
        totalAmmoText.SetText(totalAmmo.ToString("00"));
    }

    public void GetAmmo()
    {
        totalAmmo += 3;
    }
    private void GetAmmoUI()
    {
        int iterations = 0;
        int maxIterations = 100;

        while((ammoUI == null || currentAmmoText == null || totalAmmoText == null) && iterations < maxIterations)
        {
            if (ammoUI == null) ammoUI = GameObject.FindGameObjectWithTag("AmmoUI");
            else
            {
                currentAmmoText = ammoUI.transform.Find("CurrentAmmo").GetComponent<TextMeshProUGUI>();
                totalAmmoText = ammoUI.transform.Find("TotalAmmo").GetComponent<TextMeshProUGUI>();
            }
            iterations++;
        }
    }

    public bool IsReloading()
    {
        return reloading;
    }
}
