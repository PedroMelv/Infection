using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomArea : MonoBehaviour
{
    public Color roomColor;

    [Space]
    public LayerMask groundLayer;

    [Space]
    [Header("Debug")]
    [SerializeField] private bool debugPoints;
    [SerializeField] private Vector3[] points;

    public RoomPoint GetRandomPoint()
    {
        Vector3 randomPos = Vector3.zero;
        bool hitGround = false;

        int iterations = 0;
        int maxIterations = 10000;

        while (hitGround == false && iterations < maxIterations)
        {
            iterations++;

            randomPos = Vector3.zero;
            randomPos.x += Random.Range(-transform.localScale.x / 2f, transform.localScale.x / 2f);
            randomPos.z += Random.Range(-transform.localScale.z / 2f, transform.localScale.z / 2f);

            Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles);
            randomPos = rot * randomPos;

            randomPos = transform.position + randomPos;


            hitGround = Physics.Raycast(randomPos, Vector3.down, 100f, groundLayer);

            if (hitGround == false && iterations >= maxIterations) return new RoomPoint(Vector3.zero, false);
        }

        return new RoomPoint(randomPos, false);
    }

    public RoomPoint GetExtraPoint(Vector3 posBase, float dist)
    {
        Vector3 randomPos = Vector3.zero;
        bool hitGround = false;

        int iterations = 0;
        int maxIterations = 10000;

        while (hitGround == false && iterations < maxIterations)
        {
            iterations++;

            randomPos = posBase;
            randomPos.x += Random.Range(-dist, dist);
            randomPos.z += Random.Range(-dist, dist);
            

            hitGround = Physics.Raycast(randomPos, Vector3.down, 100f, groundLayer);

            if (hitGround == false && iterations >= maxIterations) return new RoomPoint(Vector3.zero, false);
        }

        return new RoomPoint(randomPos, true);
    }

    public RoomPoint[] GetRandomPoints(int amount, float distOfPoints = 0.0f, bool hasExtra = false, int extraMin = 0, int extraMax = 2, float extraMaxDist = 1f)
    {
        List<RoomPoint> points = new List<RoomPoint>();
        int amountCount = 0;

        extraMax++;

        while (amountCount < amount)
        {
            RoomPoint point = GetRandomPoint();
            
            while(point.pos == Vector3.zero)
            {
                point = GetRandomPoint();
            }

            points.Add(point);
            amountCount++;

            if(hasExtra)
            {
                int extraAmount = Random.Range(extraMin, extraMax);
                int extraCount = 0;

                while (extraCount < extraAmount)
                {
                    RoomPoint extraPoint = GetExtraPoint(point.pos, extraMaxDist);

                    while (extraPoint.pos == Vector3.zero)
                    {
                        extraPoint = GetExtraPoint(point.pos, extraMaxDist);
                    }
                    points.Add(extraPoint);

                    extraCount++;
                }
            } 
        }

        return points.ToArray();
    }

    [ContextMenu("GeneratePoints")]
    private void GeneratePoints()
    {
        int rPoints = Random.Range(2, 6);

        points = new Vector3[rPoints];

        for (int i = 0; i < rPoints; i++)
        {
            points[i] = GetRandomPoint().pos;
        }
    }

    private void OnDrawGizmos()
    {
        if(debugPoints)
        {
            Gizmos.color = Color.white;

            for (int i = 0; i < points.Length; i++)
            {
                Gizmos.DrawWireSphere(points[i], .35f);
            }
        }

        Gizmos.color = roomColor;
        
        Quaternion rot = Quaternion.Euler(transform.rotation.eulerAngles);

        // Calcule a matriz de transformação com base na posição, rotação e escala
        Matrix4x4 matrix = Matrix4x4.TRS(transform.position, rot, transform.localScale);

        // Defina a matriz de transformação para ser usada pelo Gizmos
        Gizmos.matrix = matrix;

        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        
    }
}


public struct RoomPoint
{
    public Vector3 pos;
    public bool isExtra;

    public RoomPoint(Vector3 pos, bool isExtra)
    {
        this.pos = pos;
        this.isExtra = isExtra;
    }
}
