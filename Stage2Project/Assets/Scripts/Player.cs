using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [SerializeField]
    private float Speed;

    private Rigidbody mBody;

    void Awake()
    {
        mBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector3 direction = Vector3.zero;

        // Changed input to reference Axis rather than hardcoded keys
        if (Input.GetButton("Horizontal"))
        {
            direction += Input.GetAxisRaw("Horizontal") * Vector3.right;
        }

        if (Input.GetButton("Vertical"))
        {
            direction += Input.GetAxisRaw("Vertical") * Vector3.forward;
        }

        mBody.velocity = (direction * Speed);
    }
}
