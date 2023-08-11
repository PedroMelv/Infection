using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubmarineTest : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;
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
    void Update()
    {
        transform.Translate(transform.forward * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void SubmarineInputRotation()
    {
        rotateSpeed = Random.Range(-40f, 40f);
    }

    private void SubmarineInputMovement()
    {
        moveSpeed = Random.Range(-8f, 8f);
    }
}
