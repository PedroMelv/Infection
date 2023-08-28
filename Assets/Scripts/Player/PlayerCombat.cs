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

    private void Awake()
    {
        pInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        isAiming = pInput.rightMouseInput;

        if(isAiming && pInput.leftMouseInputPressed)
        {
            Ray mouseRay = pInput.myCamera.ScreenPointToRay(Input.mousePosition);
            bool collided = Physics.Raycast(mouseRay, out RaycastHit hit, 100f, hitLayer);

            if(collided)
            {
                GameObject mark = PhotonNetwork.Instantiate(bulletMark.name, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                mark.transform.position += mark.transform.forward * 0.01f;
            }
        }
    }
}
