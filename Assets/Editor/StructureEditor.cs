using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Structure))]
public class StructureEditor : Editor
{
    private float updateTimer;

    public override void OnInspectorGUI()
    {
        Structure st = (Structure)target;

        if (updateTimer <= 0f)
        {
            
            for (int i = 0; i < st.doors.Count; i++)
            {
                if (st.doors[i].doorLocation == null)
                {
                    st.RemoveDoor(i);
                    break;
                }
                if (st.doors[i].autoSetDirection)
                {
                    Vector3 doorCenterPos = (st.doors[i].doorCenter == null) ? st.transform.position : st.doors[i].doorCenter.position;

                    //Setting Door direction
                    Vector3 directions = st.doors[i].doorLocation.position - doorCenterPos;

                    Direction dir = Direction.NORTH;

                    if (Mathf.Abs(directions.x) > Mathf.Abs(directions.z))
                    {
                        if (directions.x > 0f) dir = Direction.NORTH; else dir = Direction.SOUTH;
                    }
                    else
                    {
                        if (directions.z > 0f) dir = Direction.EAST; else dir = Direction.WEST;
                    }

                    st.doors[i].doorDirection = dir;
                }
            }
            updateTimer = 1f;
        }
        else
        {
            updateTimer -= Time.deltaTime;
        }
        
        if(GUILayout.Button("Add Door"))
        {
            GameObject newDoor = new GameObject("Door");

            newDoor.transform.SetParent(st.gameObject.transform);

            var iconContent = EditorGUIUtility.IconContent("sv_label_1");
            EditorGUIUtility.SetIconForObject(newDoor, (Texture2D)iconContent.image);

            st.AddDoor(newDoor);
        }
        
        GUILayout.Space(EditorGUIUtility.singleLineHeight);
        base.OnInspectorGUI();

    }
}
