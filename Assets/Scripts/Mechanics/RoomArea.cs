using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomArea : MonoBehaviour
{
    public Color roomColor;
    [Space]
    public Vector3 offset;
    public Vector3 size;
    [Space]
    public LayerMask groundLayer;

    public Vector3 GetRandomPoint()
    {
        Vector3 randomPos = Vector3.zero;

        bool hitGround = false;

        RaycastHit hit = new RaycastHit();

        int iterations = 0;
        int maxIterations = 10000;

        while (hitGround == false && iterations < maxIterations)
        {
            iterations++;

            randomPos = transform.position + offset;
            randomPos.x += Random.Range(-size.x / 2f, size.x / 2f);
            randomPos.z += Random.Range(-size.z / 2f, size.z / 2f);

            hitGround = Physics.Raycast(randomPos, Vector3.down, out hit, 100f, groundLayer);
        }

        return hit.point;
    }

    public Vector3[] GetRandomPoints(int amount, float distOfPoints = 0.0f)
    {
        int fill = 0;

        Vector3[] result = new Vector3[amount];

        int fillIteration = 0;
        int maxFillIterations = 1000;

        while (fill < amount)
        {
            Vector3 point = GetRandomPoint();

            bool hasPointClose = false;

            for (int i = 0; i < result.Length; i++)
            {
                if (Vector3.Distance(point, result[i]) < distOfPoints && result[i] != Vector3.zero)
                {
                    hasPointClose = true;
                }
            }

            if(fillIteration < maxFillIterations)
            {
                if (hasPointClose == false)
                {
                    result[fill++] = point;
                    fillIteration = 0;
                }
                else
                {
                    fillIteration++;
                }
            }
            else
            {
                result[fill++] = Vector3.zero;
                fillIteration = 0;
            }

            
        }

        return result;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = roomColor;

        Gizmos.DrawWireCube(transform.position + offset, size);
        
    }
}
