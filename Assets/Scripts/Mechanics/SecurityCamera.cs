using Photon.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Camera Rotation")]
    private Camera myCamera;
    [SerializeField] private float rotationMinAngle;
    [SerializeField] private float rotationMaxAngle;
    [SerializeField, Range(0f,360f)] private float rotationCenter;


    private float yInput = 0f;
    private float yRotation = 0f;

    private void Start()
    {
        myCamera = GetComponentInChildren<Camera>();
        myCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        //if (myCamera.gameObject.activeSelf == false) return;

        float input = GetMouseInput();
        yInput = Mathf.Lerp(yInput, input, 5f * Time.deltaTime);

        yRotation += yInput * 2.5f;

        yRotation = Mathf.Clamp(yRotation, 
            rotationCenter + rotationMinAngle + 45 , 
            rotationCenter + rotationMaxAngle - 45 );

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yRotation, transform.eulerAngles.z);
    }

    private float GetMouseInput()
    {
        float mousePos = myCamera.ScreenToViewportPoint(Input.mousePosition).x;

        if (mousePos <= .1f) return -1f;
        if (mousePos >= .9f) return  1f;
        return 0f;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }


    public void SetCameraActive(bool active)
    {
        myCamera.gameObject.SetActive(active);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + DirFromAngle(rotationCenter+rotationMinAngle, true) );
        Gizmos.DrawLine(transform.position, transform.position + DirFromAngle(rotationCenter+rotationMaxAngle, true) );
    }
}
