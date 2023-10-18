using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Structure : MonoBehaviour
{
    public List<DoorPosition> doors;

    public void AddDoor(GameObject doorLocation)
    {
        DoorPosition newDoor = new DoorPosition();

        newDoor.doorLocation = doorLocation.transform;
        doorLocation.transform.position = transform.position;
        doors.Add(newDoor);
    }
    public void RemoveDoor(int at)
    {
        doors.RemoveAt(at);
    }
}

public enum Direction
{
    NORTH = 0,
    SOUTH = 1,
    EAST = 2,
    WEST = 3
}

[System.Serializable]
public class DoorPosition
{
    public Transform doorLocation;
    [Tooltip("Usado para obter a direção da porta")]
    public Transform doorCenter;
    [Tooltip("Relative to world's north: X+")]
    public bool autoSetDirection;
    public Direction doorDirection;
}