using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkLight : MonoBehaviour
{
    private Light lightComponent;

    private float timer;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
    }

    private void Update()
    {
        if(timer <= 0f)
        {
            lightComponent.enabled = !lightComponent.enabled;
            timer = Random.Range(.05f, .75f);
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }
}
