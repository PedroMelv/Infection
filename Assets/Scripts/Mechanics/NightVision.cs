using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NightVision : MonoBehaviour
{
    [SerializeField] private Volume nightVisionVolume;
    [SerializeField] private Color defaultAmbienceColor;
    [SerializeField] private Color brightAmbienceColor;

    private bool nightVision;

    public static NightVision Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        defaultAmbienceColor = RenderSettings.ambientLight;
    }

    public void TriggerNightVision(bool isActive)
    {
        nightVision = isActive;
        nightVisionVolume.weight = isActive ? 1.0f : 0.0f;
        RenderSettings.ambientLight = isActive ? brightAmbienceColor : defaultAmbienceColor;
        RenderSettings.fog = !isActive;
    }

}
