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

    void FixedUpdate()  // Changed to FixedUpdate as influencing Rigidbody
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

        Vector3 gravity = Vector3.down * GameManager.Get.gravity * mBody.mass;

        // Maintaining use of Rigidbody force rather than Translation
        mBody.velocity = (Vector3.up * mBody.velocity.y) + (direction * Speed);
        mBody.AddForce(gravity);
    }
}
