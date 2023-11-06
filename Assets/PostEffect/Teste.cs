using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable]
public class Teste : VolumeComponent, IPostProcessComponent
{
    public bool IsActive()
    {
        throw new NotImplementedException();
    }

    public bool IsTileCompatible()
    {
        throw new NotImplementedException();
    }
}
