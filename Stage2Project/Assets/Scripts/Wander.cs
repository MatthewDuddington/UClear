using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour {

    [SerializeField]
    private float WanderStrength = 100;
    [SerializeField]
    private float WanderUpdateTime = 0.1f;
    private WaitForSeconds waitForWanderUpdateTime;

    private Agent mAgent;

    [SerializeField]
    private float futureDistance = 3;
    [SerializeField]
    private float futureRadius = 1.5f;
    [SerializeField]
    private float minRadianOffset = -0.5f;
    [SerializeField]
    private float maxRadianOffset = 0.5f;

    private float wanderRadianAngle;

    void Awake()
    {
        mAgent = GetComponent<Agent>(); 
        waitForWanderUpdateTime = new WaitForSeconds(WanderUpdateTime);
    }

    void Start()
    {
        StartCoroutine(Co_Wander());
    }

    private IEnumerator Co_Wander()
    {
        while (enabled)
        {
            Vector3 futureCentre = transform.position + (transform.forward * futureDistance);

            float randomRadianOffset = Random.Range(minRadianOffset, maxRadianOffset);
            wanderRadianAngle += randomRadianOffset;

            Vector3 targetPoint = new Vector3(futureRadius * Mathf.Cos(wanderRadianAngle), 0, futureRadius * Mathf.Sin(wanderRadianAngle)) + futureCentre;
            Debug.DrawLine(transform.position, targetPoint, Color.red, WanderUpdateTime);

            Vector3 wanderForce = (-transform.position + targetPoint).normalized;
            wanderForce *= WanderStrength;
            mAgent.WanderForce = wanderForce;

            yield return waitForWanderUpdateTime;
        }
    }
}
