using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private LayerMask hitLayer;

    [SerializeField] private GameObject bulletMark;

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

            if(collided)
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
}
