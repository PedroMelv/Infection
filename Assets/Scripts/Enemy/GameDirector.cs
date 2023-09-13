using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [Header("Areas")]
    [SerializeField] private RoomArea wholeArea;
    [SerializeField] private List<RoomArea> areas;

    public static GameDirector instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GetAreas();
    }

    private void GetAreas()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("GameArea");

        areas = new List<RoomArea>();

        for (int i = 0; i < objs.Length; i++)
        {
            RoomArea a = objs[i].GetComponent<RoomArea>();

            if(a != null && a != wholeArea)
            {
                areas.Add(a);
            }
            
        }
    }

    public RoomPoint[] GetPoints(RoomArea area,int amount, float minDist = 1f, bool hasExtra = false, int extraMin = 0, int extraMax = 2, float extraMaxDist = 1f)
    {
        return area.GetRandomPoints(amount, minDist, hasExtra, extraMin, extraMax, extraMaxDist);
    }

    public RoomPoint[] GetClosestPoints(Vector3 from, int amount, float minDist = 1f, bool hasExtra = false, int extraMin = 0, int extraMax = 2, float extraMaxDist = 1f)
    {
        return GetPoints(GetClosestRoom(from), amount, minDist, hasExtra, extraMin, extraMax, extraMaxDist);
    }
    
    public RoomPoint[] GetRandomPoints(int amount, float minDist = 1f, bool hasExtra = false, int extraMin = 0, int extraMax = 2, float extraMaxDist = 1f)
    {
        return GetPoints(GetRandomRoom(), amount, minDist, hasExtra, extraMin, extraMax, extraMaxDist);
    }
    
    public RoomPoint GetRandomPointOnMap()
    {
        int selected = 0;
        RoomPoint[] GotPoints = GetPoints(wholeArea, 5, 1f, false, 0, 0, 0);

        for (int i = 0; i < GotPoints.Length; i++)
        {
            if (GotPoints[i].pos != Vector3.zero)
            {
                selected = i;
                break;
            }
        }

        return GotPoints[selected];
    }

    public RoomArea GetClosestRoom(Vector3 from)
    {
        RoomArea selRoom = null;
        float dist = int.MaxValue;

        for (int i = 0; i < areas.Count; i++)
        {
            if (Vector3.Distance(areas[i].transform.position, from) < dist)
            {
                selRoom = areas[i];
                dist = Vector3.Distance(areas[i].transform.position, from);
            }
        }

        return selRoom;
    }

    public RoomArea GetRandomRoom()
    {
        return areas[Random.Range(0, areas.Count)];
    }
}

