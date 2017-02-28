using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidWalls : MonoBehaviour {

    [SerializeField]
    private float AvoidanceStrength = 100;

    [SerializeField]
    private float AvoidanceRadius = 1.5f;

    private Agent mAgent;

    [SerializeField]
    private float avoidWallsTime = 2.7f;
    private WaitForSeconds waitForAvoidWallsTime;

    private bool isAvoiding = false;

    private Vector3 avoidDirection;

    void Awake()
    {
        mAgent = GetComponent<Agent>();
        waitForAvoidWallsTime = new WaitForSeconds(avoidWallsTime);
    }

    void Start()
    {

    }

    public void UpdateBehaviour()
    {
        mAgent.AvoidWallsForce = Vector3.zero;

        foreach(RaycastHit hit in mAgent.hits)
        {
            if (hit.collider != null)
            {    
                // When coliding with a wall create a vector pointing away from the wall 
                if (hit.collider.CompareTag("Wall"))
                {
                    Vector3 wallNormal = hit.collider.transform.right;
                    float proximityMagnitude = 1 / (((hit.distance * hit.distance) * AvoidanceStrength) + 1);
                    mAgent.AvoidWallsForce += wallNormal * proximityMagnitude ;
                }
            }
        }
    }

    // When coliding with a wall create a vector pointing away from the wall 
//    void OnCollisionEnter(Collision coll)
//    {
//        if (coll.gameObject.CompareTag("Wall"))
//        {
//            // Allow for corner wall cases by += on different hits
//            avoidDirection += coll.gameObject.transform.right;  // All wall red axies point inward
//            avoidDirection.Normalize();
//
//            if (!isAvoiding)  // Prevent multiple copies of the coroutine running
//            {
//                isAvoiding = true;
//                StartCoroutine(Co_AvoidWalls());
//            }
//        }
//    }
//
//    private IEnumerator Co_AvoidWalls()
//    {
//        mAgent.AvoidWallsForce = avoidDirection * AvoidanceStrength;
//        yield return waitForAvoidWallsTime;
//        avoidDirection = Vector3.zero;
//        mAgent.AvoidWallsForce = Vector3.zero;
//        isAvoiding = false;
//    }
}
