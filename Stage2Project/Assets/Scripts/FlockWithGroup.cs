using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlockWithGroup : MonoBehaviour
{
    [SerializeField]
    private GroupTag.Group GroupCode;

//    [SerializeField]
//    private float Speed;  // Moved to Agent class

    [SerializeField]
    private float BuddyDistance = 100.0f;

    [SerializeField]
    private float AvoidDistance = 1.0f;

    [SerializeField]
    private float CheckForBuddiesInterval = 10.0f;

    private List<GroupTag> mCurrentBuddies;
//    private Rigidbody mBody;
    private Agent mAgent;
    private float mCountDownToCheck;

    void Awake()
    {
        mCurrentBuddies = new List<GroupTag>();
//        mBody = GetComponent<Rigidbody>();
        mAgent = GetComponent<Agent>();
        mCountDownToCheck = 0.0f;
    }

    void FixedUpdate()
    {
        mCountDownToCheck -= Time.fixedDeltaTime;
        if (mCountDownToCheck <= 0.0f)
        {
            UpdateBuddyList();
            mCountDownToCheck = CheckForBuddiesInterval;
        }

        FlockWithBuddies();
    }

    private void UpdateBuddyList()
    {
        GroupTag[] individuals = FindObjectsOfType<GroupTag>();

        for (int count = 0; count < individuals.Length; ++count)
        {
            if (individuals[count].gameObject != gameObject && individuals[count].Affiliation == GroupCode )
            {
                Vector3 difference = individuals[count].transform.position - transform.position;
                if (difference.magnitude <= BuddyDistance)
                {
                    if (!mCurrentBuddies.Contains(individuals[count]))
                    {
                        mCurrentBuddies.Add(individuals[count]);
                    }
                }
                else if (mCurrentBuddies.Contains(individuals[count]))
                {
                    mCurrentBuddies.Remove(individuals[count]);
                }
            }
        }
    }

    private void FlockWithBuddies()
    {
        if (mCurrentBuddies.Count > 0)
        {
            Vector3 align = Vector3.zero;
            Vector3 cohesion = Vector3.zero; 
            Vector3 avoid = Vector3.zero;
            
            for (int count = 0; count < mCurrentBuddies.Count; ++count)
            {
                Rigidbody body = mCurrentBuddies[count].GetComponent<Rigidbody>();
                align += body.velocity;
                cohesion += mCurrentBuddies[count].transform.position;
                if ( ( mCurrentBuddies[count].transform.position - transform.position ).magnitude < AvoidDistance)
                {
                    avoid += mCurrentBuddies[count].transform.position;
                }
            }

            align /= mCurrentBuddies.Count;
            cohesion /= mCurrentBuddies.Count;
            avoid /= mCurrentBuddies.Count;

            align.Normalize();
            cohesion = cohesion - transform.position;
            cohesion.Normalize();
            avoid = transform.position - avoid;
            avoid.Normalize();

            mAgent.FlockVector = (align + cohesion + avoid).normalized;  // Changed to submit resulting flocking force to the main agent script to resolve with weighting
        }
    }
}
