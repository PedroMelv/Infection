using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Billboard : MonoBehaviour
{
    [SerializeField] private bool lockX, lockY, lockZ;
    private Camera cam;

    private void Update()
    {
        if(cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }
        else
        {
            Vector3 eulerAngle = cam.transform.eulerAngles;

            if (lockX) eulerAngle.x = 0;
            if (lockY) eulerAngle.y = 0;
            if (lockZ) eulerAngle.z = 0;

            this.gameObject.transform.rotation = Quaternion.Euler(eulerAngle);

        }
    }
}
