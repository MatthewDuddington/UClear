using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntPlayer : MonoBehaviour {

    [SerializeField]
    private float HuntStrength = 100;
    
    private Agent mAgent;

    public float PlayerSightingDecayTime = 4;
    private float PlayerSightingTime;

    public bool HasSeenPlayer { get; private set; }

    private Vector3 lastKnownLocation;

    void Awake()
    {
        mAgent = GetComponent<Agent>();

        HasSeenPlayer = false;
    }

    void Start()
    {

    }

    public void UpdateBehaviour()
    {
        foreach(RaycastHit hit in mAgent.hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Player"))
                {
                    HasSeenPlayer = true;
                    PlayerSightingTime = Time.fixedTime + PlayerSightingDecayTime;
                    lastKnownLocation = hit.collider.transform.position;
                }
            }
        }
        if (Time.fixedTime > PlayerSightingTime)
        {
            HasSeenPlayer = false;
        }

        if (HasSeenPlayer)
        {
            mAgent.HuntForce = (-transform.position + lastKnownLocation).normalized * HuntStrength;
        }
        else
        {
            mAgent.HuntForce = Vector3.zero;
        }
    }
}
