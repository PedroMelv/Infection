using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtils
{
    public static void RecursiveSetLayer(this Transform startTransform, int layer)
    {
        startTransform.gameObject.layer = layer;

        for (int i = 0; i < startTransform.childCount; i++)
        {
            startTransform.GetChild(i).gameObject.layer = layer;
            if (startTransform.GetChild(i).childCount != 0) RecursiveSetLayer(startTransform.GetChild(i), layer);
        }
    }

    public static void RecursiveSetLayer(this Transform startTransform, string layer)
    {
        RecursiveSetLayer(startTransform, LayerMask.NameToLayer(layer));
    }
}
