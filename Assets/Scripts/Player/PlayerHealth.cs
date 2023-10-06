using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerHealth : BaseHealth
{
    private Volume damageVolume;

    public override void Start()
    {
        base.Start();
        damageVolume = GameObject.FindGameObjectWithTag("DamageVolume").GetComponent<Volume>();
    }

    public override void Update()
    {
        base.Update();

        if(photonView.IsMine)damageVolume.weight = Mathf.Lerp(damageVolume.weight, 1 - health / maxHealth, 5f * Time.deltaTime);
    }
}
