using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviourPun
{
    [SerializeField] private LayerMask hitLayer;

    [SerializeField] private GameObject bulletMark;

    [SerializeField] private AudioClip shootSound;

    private bool isAiming = false;
    public bool IsAiming { get { return isAiming; } private set { } }
    private PlayerInput pInput;
    private PlayerInventory pInventory;

    bool canUseWeapon = false;

    private void Awake()
    {
        pInput = GetComponent<PlayerInput>();
        pInventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if (pInventory.GetSelectedItem() != null && pInventory.GetSelectedItem().specialUse != SpecialUseItem.GUN) return;
        
        isAiming = pInput.rightMouseInput;

        if(isAiming && pInput.leftMouseInputPressed)
        {
            Ray mouseRay = pInput.myCamera.ScreenPointToRay(Input.mousePosition);
            bool collided = Physics.Raycast(mouseRay, out RaycastHit hit, 100f, hitLayer);

            PlayShootSound();

            if (collided)
            {
                BaseHealth hp = hit.collider.gameObject.GetComponent<BaseHealth>();

                if(hp != null) 
                { 
                    if(hp is EnemyHealth)
                    {
                        hp.CallTakeDamage(1, transform.position);
                        return;
                    }
                    return;
                }  

                GameObject mark = PhotonNetwork.Instantiate(bulletMark.name, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                mark.transform.position += mark.transform.forward * 0.025f;
                Destroy(mark, 5f);
            }
        }
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
}
