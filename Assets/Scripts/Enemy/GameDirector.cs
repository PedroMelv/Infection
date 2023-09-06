using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private RoomArea[] areas;
    [SerializeField] private int index;
    [SerializeField] private Vector3[] bunchOfPoints;

    private void Start()
    {
        GetAreas();

        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) bunchOfPoints = areas[index].GetRandomPoints(4, 5f);
    }

    private void GetAreas()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameArea");

        areas = new RoomArea[objs.Length];

        for (int i = 0; i < areas.Length; i++)
        {
            areas[i] = objs[i].GetComponent<RoomArea>();
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < bunchOfPoints.Length; i++)
        {
            Gizmos.DrawWireSphere(bunchOfPoints[i], .5f);
        }
    }
}
