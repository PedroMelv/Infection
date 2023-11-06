using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityCaneraHandler : Interactable
{
    [SerializeField] private List<SecurityCamera> cameras;
    
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject cameraCanvas;

    [SerializeField, Range(1,60)] private int cameraFrameRate;
    private float cameraTimer;
    
    private NightVision nightVision;
    private bool onCamera;
    private int currentCamera;

    protected override void Start()
    {
        base.Start();
        OnInteractAction += InteractCamera;
        nightVision = NightVision.Instance;

        TriggerCamera(false);
    }

    protected override void Update()
    {
        base.Update();
        //if (Input.GetKeyDown(KeyCode.H)) InteractCamera(null);

        if (onCamera)
        {
            if(Input.GetKeyDown(KeyCode.Escape)) CloseCamera();

            if(cameraTimer > 1f / cameraFrameRate)
            {
                cameras[currentCamera].SetCameraActive(true);
                cameraTimer = 0f;
            }
            else
            {
                cameras[currentCamera].SetCameraActive(false);
                cameraTimer += Time.deltaTime;
            }
        }
    }

    private void TriggerCamera(bool onCamera)
    {
        cameraTimer = 0f;
        this.onCamera = onCamera;

        gameCanvas.SetActive(!onCamera);
        cameraCanvas.SetActive(onCamera);

        if(lastInteract != null)lastInteract.myCamera.gameObject.SetActive(!onCamera);
        if(lastInteract != null)lastInteract.canInput = !onCamera;
        Cursor.lockState = onCamera ? CursorLockMode.None : CursorLockMode.Locked;

        cameras[currentCamera].SetCameraActive(onCamera);
        nightVision.TriggerNightVision(onCamera);
    }

    private void InteractCamera(GameObject whoInteracted)
    {
        TriggerCamera(!onCamera);
    }


    public void SetCamera(SecurityCamera camera)
    {
        cameras[currentCamera].SetCameraActive(false);
        currentCamera = cameras.IndexOf(camera);
        cameras[currentCamera].SetCameraActive(true);
    }

    public void CloseCamera()
    {
        TriggerCamera(false);
    }
}
