using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    [SerializeField] private FieldOfView focusedView;

    [SerializeField] private FieldOfView unfocusedView;
    [SerializeField, Range(0f,1f)] private float unfocusedDetectionStrength;
    [SerializeField] private FieldOfView periphericalView;
    [SerializeField, Range(0f,1f)] private float periphericalDetectionStrength;
    
    [SerializeField] private FieldOfView closeView;

    [SerializeField] private float detectionMaxValue;
    [SerializeField] private float detectionValue;

    private Transform detectedTarget;
    private Transform storedTarget;

    private EnemyBrain brain;

    private void Awake()
    {
        brain = GetComponent<EnemyBrain>();
    }

    private void Update()
    {
        HandleVision();

        brain.TriggerVision(detectedTarget);
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

        if(first == null)
        {
            detectionValue = 0f;
            storedTarget = null;
            detectedTarget = null;
        }

        #endregion
    }

    public Transform GetDetectedTarget()
    {
        return detectedTarget;
    }
}
