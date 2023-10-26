using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [Header("Heatmap")]
    [SerializeField] private int maxHeatmapTrackAmount;
    [SerializeField] private float heatmapTrackTimer;
    [SerializeField] private List<HeatmapData> heatmaps = new List<HeatmapData>();
    private float heatmapTimer;

    private RoomArea lastRoomArea;

    private class HeatmapData
    {
        public GameObject playerTrackObj;

        public List<Vector3> positions = new List<Vector3>();

        public Vector3 GetHeatmapPos()
        {
            if(positions.Count < 15) return Vector3.zero;

            int minTrack = Random.Range(0, positions.Count - 10);
            int maxTrack = Random.Range(minTrack, positions.Count);
            int amountOfTracking = maxTrack - minTrack;

            Vector3 totalPositions = Vector3.zero;

            for (int i = minTrack; i < maxTrack; i++)
            {
                totalPositions += positions[i];
            }

            totalPositions = totalPositions / amountOfTracking;

            return totalPositions;
        }
    }

    [Header("Areas")]
    [SerializeField] private RoomArea wholeArea;
    [SerializeField] private List<RoomArea> areas;

    public static GameDirector instance;

    private void Awake()
    {
        if(PhotonNetwork.IsMasterClient == false && PhotonNetwork.InRoom == true)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        GetAreas();
    }

    private void Update()
    {
        UpdateHeatmapTracking();
    }

    private void StartHeatmap()
    {

    }
    private void UpdateHeatmapTracking()
    {
        if(heatmapTimer <= 0f)
        {
            if (heatmaps.Count == 0)
            {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (players.Length == 2)
                {
                    for (int i = 0; i < players.Length; i++)
                    {
                        HeatmapData data = new HeatmapData();
                        data.playerTrackObj = players[i];
                        heatmaps.Add(data);
                    }
                }
            }
            else
            {
                for (int i = 0; i < heatmaps.Count; i++)
                {
                    Vector3 randomIncrease = Random.insideUnitSphere * 5f;
                    randomIncrease.y = 0f;
                    heatmaps[i].positions.Add(heatmaps[i].playerTrackObj.transform.position + randomIncrease);
                }
                if (heatmaps.Count >= maxHeatmapTrackAmount)
                {
                    heatmaps.RemoveAt(0);
                }
            }

            heatmapTimer = heatmapTrackTimer;
        }
        else
        {
            heatmapTimer -= Time.deltaTime;
        }

        
    }

    public Vector3 GetHeatmapPos(int trackIndex)
    {
        return heatmaps[trackIndex].GetHeatmapPos();
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
    
    public RoomPoint[] GetFurthestPoints(Vector3 from, int amount, float minDist = 1f, bool hasExtra = false, int extraMin = 0, int extraMax = 2, float extraMaxDist = 1f)
    {
        return GetPoints(GetFurthestRoom(from), amount, minDist, hasExtra, extraMin, extraMax, extraMaxDist);
    }
    
    public RoomPoint[] GetRandomPoints(int amount, float minDist = 1f, bool hasExtra = false, int extraMin = 0, int extraMax = 2, float extraMaxDist = 1f)
    {
        return GetPoints(GetRandomRoom(), amount, minDist, hasExtra, extraMin, extraMax, extraMaxDist);
    } 
    public RoomPoint GetRandomPointOnMap()
    {
        int selected = 0;
        RoomPoint[] GotPoints = GetPoints(wholeArea, 50, 1f, false, 0, 0, 0);

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

        List<RoomArea> listAreas = new List<RoomArea>(areas);
        if(lastRoomArea != null) listAreas.Remove(lastRoomArea);

        for (int i = 0; i < listAreas.Count; i++)
        {
            if (Vector3.Distance(listAreas[i].transform.position, from) < dist)
            {
                selRoom = listAreas[i];
                dist = Vector3.Distance(listAreas[i].transform.position, from);
            }
        }

        lastRoomArea = selRoom;

        return selRoom;
    }
    
    public RoomArea GetFurthestRoom(Vector3 from)
    {
        RoomArea selRoom = null;
        float dist = int.MinValue;

        for (int i = 0; i < areas.Count; i++)
        {
            if (Vector3.Distance(areas[i].transform.position, from) > dist)
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

