using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineTest : MonoBehaviour
{
    public float rotateSpeed;
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        InvokeRepeating("SubmarineInputMovement", 0f, 5f);
        InvokeRepeating("SubmarineInputRotation", 0f, 5f);
    }
    void FixedUpdate()
    {
        transform.position += transform.forward * moveSpeed * Time.fixedDeltaTime;
        transform.Rotate( Vector3.up * rotateSpeed * Time.fixedDeltaTime);
    }

    private void SubmarineInputRotation()
    {
        rotateSpeed = Random.Range(-60f, 60f);
    }

    private void SubmarineInputMovement()
    {
        moveSpeed = Random.Range(-8f, 8f);
    }
}
