using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : MonoBehaviour {

    [SerializeField]
    private float futureDistance = 3;
    [SerializeField]
    private float futureRadius = 1.5f;
    [SerializeField]
    private float minRadianOffset = 0.01f;
    [SerializeField]
    private float maxRadianOffset = 0.5f;

    private Vector3 targetPoint;

    private Agent mAgent;

    [SerializeField]
    private float wanderUpdateTime = 0.1f;
    private WaitForSeconds waitForWanderUpdateTime;

    void Awake()
    {
        mAgent = GetComponent<Agent>(); 
        waitForWanderUpdateTime = new WaitForSeconds(wanderUpdateTime);
    }

    void Start()
    {
        StartCoroutine(Co_Wander());
    }

    private IEnumerator Co_Wander()
    {
        while (enabled)
        {
            Vector3 futurePoint = transform.position + (transform.forward * futureDistance);

            float randomRadianOffset = Random.Range(minRadianOffset, maxRadianOffset);
            float horizontalPos = futureRadius * Mathf.Cos(randomRadianOffset) + futurePoint.x;
            float verticalPos = futureRadius * Mathf.Sin(randomRadianOffset) + futurePoint.z;
            targetPoint = new Vector3(horizontalPos, 0, verticalPos);

            Vector3 wanderVector = (-transform.position + targetPoint).normalized;

            mAgent.WanderVector = wanderVector;
            yield return waitForWanderUpdateTime;
        }
    }
}
