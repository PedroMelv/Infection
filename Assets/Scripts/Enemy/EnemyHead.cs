using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyHead : MonoBehaviourPun
{
    [Header("Sight")]
    [SerializeField] private FieldOfView focusedView;

    [SerializeField] private FieldOfView unfocusedView;
    [SerializeField, Range(0f,1f)] private float unfocusedDetectionStrength;
    [SerializeField] private FieldOfView periphericalView;
    [SerializeField, Range(0f,1f)] private float periphericalDetectionStrength;
    
    [SerializeField] private FieldOfView closeView;

    [SerializeField] private float detectionMaxValue;
    [SerializeField] private float detectionValue;

    [Header("Audition")]
    [SerializeField] private float auditionRange;
    [SerializeField] private LayerMask auditionLayerMask;
    

    private Transform detectedTarget;
    private Transform storedTarget;

    private EnemyBrain brain;

    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
    }

    private void Update()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;

        HandleVision();
        HandleAudition();

        brain.TriggerVision(detectedTarget);
    }


    private void HandleAudition()
    {
        Collider[] auditionDetection = Physics.OverlapSphere(transform.position, auditionRange, auditionLayerMask);

        int closest = -1;
        float dist = float.MaxValue;

        if(auditionDetection.Length == 0 )
        {
            return;
        }

        for (int i = 0; i < auditionDetection.Length; i++)
        {
            float curDistance = Vector3.Distance(auditionDetection[i].transform.position, transform.position);
            if (curDistance < dist)
            {
                dist = curDistance;
                closest = i;
            }
        }

        brain.TriggerVision(auditionDetection[closest].transform.position);
    }

    private void HandleVision()
    {
        Transform first = null;

        #region Detecção de maior prioridade

        if (focusedView != null && focusedView.GetFirstTarget(out first))
        {
            if (detectedTarget != first) detectionValue = 0f;

            detectedTarget = first;
            storedTarget = first;
            return;
        }

        if(closeView != null && closeView.GetFirstTarget(out first))
        {
            if (detectedTarget != first) detectionValue = 0f;

            detectedTarget = first;
            storedTarget = first;
            return;
        }

        #endregion

        #region Detecções auxiliares

        if(unfocusedView != null && unfocusedView.GetFirstTarget(out first))
        {
            if (storedTarget != first) detectionValue = 0f;

            storedTarget = first;

            if(detectionValue > detectionMaxValue)
            {
                detectedTarget = first;
            }
            else
            {
                detectionValue += Time.deltaTime * unfocusedDetectionStrength;
            }

            return;
        }

        if (periphericalView != null && periphericalView.GetFirstTarget(out first))
        {
            if (storedTarget != first) detectionValue = 0f;

            storedTarget = first;

            if (detectionValue > detectionMaxValue)
            {
                detectedTarget = first;
            }
            else
            {
                detectionValue += Time.deltaTime * periphericalDetectionStrength;
            }

            return;
        }


        #endregion
        
        if(first == null)
        {
            detectionValue = 0f;
            storedTarget = null;
            detectedTarget = null;
        }
    }

    public Transform GetDetectedTarget()
    {
        return detectedTarget;
    }
}
