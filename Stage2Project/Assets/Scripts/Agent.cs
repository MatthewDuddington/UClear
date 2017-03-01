using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{ 
    public enum BehaviourState { Disabled, Wandering, Hunting }
    private BehaviourState mState;

    static public int ActiveAgentsCount { get; private set; }

    static public float ColliderRadius { get; private set; }

    static private float desisionTickTime = 0.3f;
    static private WaitForSeconds waitForDesisionTick = new WaitForSeconds(desisionTickTime);

    static private GameObject agentPrefab;  // Remove once randomised function written

    public RaycastHit[] hits { get; private set; }

    private Vector3 gravity;

    public Vector3 FlockForce;  //{ get; set; }
    public Vector3 WanderForce; //{ get; set; }
    public Vector3 AvoidWallsForce; //{ get; set; }
    public Vector3 HuntForce;   //{ get; set; }

    private HuntPlayer mHunt;
    private FlockWithGroup mFlock;
    private Wander mWander;
    private AvoidWalls mAvoid;

    private Rigidbody mBody;

    // TODO Load in different agent bodyparts and return a randomised character
    static public GameObject GenerateRandomAgentDesign()
    {
        if (agentPrefab == null)
        {
            agentPrefab = Resources.Load<GameObject>("Boffin_M");
        }
        GameObject randomAgent = agentPrefab;
        return randomAgent;
    }

    void Awake()
    {
        mBody = gameObject.GetComponent<Rigidbody>();
        ColliderRadius = GetComponent<CapsuleCollider>().radius;
        hits = new RaycastHit[16];

        mHunt = GetComponent<HuntPlayer>();
        mFlock = GetComponent<FlockWithGroup>();
        mWander = GetComponent<Wander>();
        mAvoid = GetComponent<AvoidWalls>();

        ActiveAgentsCount++;
        Reset();
//        StartCoroutine(Co_DecisionTick());  // TODO Remove
    }

    void Start()
    {
        gravity = Vector3.down * GameManager.Get.gravity * mBody.mass;
    }

    public void Reset()
    {
        gameObject.SetActive(false);              // Disable components
        ActiveAgentsCount--;                      // Remove from count of agents in play
        transform.position = Vector3.down * 100;  // Store agent outside play area

        // Clear steering force components
        FlockForce = Vector3.zero;
        WanderForce = Vector3.zero;
        AvoidWallsForce = Vector3.zero;
        HuntForce = Vector3.zero;

        mState = BehaviourState.Disabled; // Put behaviour machine in hibernation
    }

    public void Init()
    {
        gameObject.SetActive(true);  // Enable components
        ActiveAgentsCount++;         // Add to count of agents in play

        // Place agent on spawn point
        mBody.MovePosition(Map.Get.AgentSpawnLocation);
        mBody.MoveRotation(Quaternion.identity);

        mState = BehaviourState.Wandering;  // Set agent to wander state

        StartCoroutine(Co_DecisionTick());  // Run behaviour machine
    }

    // Coroutiene behaviour machine, handling global awareness and state switching
    private IEnumerator Co_DecisionTick()
    {
        while (mState != BehaviourState.Disabled)
        {
            for (int i = 0; i < 16; i++)
            {
                Vector3 rayDirection = new Vector3(Mathf.Cos(22.5f * i), 0, Mathf.Sin(22.5f * i));
                Physics.Raycast(transform.position + Vector3.up, rayDirection, out hits[i]);
                Debug.DrawRay(transform.position + Vector3.up, rayDirection, Color.blue, desisionTickTime);
            }

            if (HuntForce.sqrMagnitude > 0)
            {
                mState = BehaviourState.Hunting;
            }
            else
            {
                mState = BehaviourState.Wandering;
            }

            yield return waitForDesisionTick;
        }
    }

    void FixedUpdate()
    {
        // Add forces according to desision state
        Vector3 totalForce = Vector3.zero;

        mHunt.UpdateBehaviour();  // Check if player has been seen and calculate steering force contribution
        totalForce += HuntForce;

        if (mState == BehaviourState.Wandering)
        {
            totalForce += FlockForce;

            totalForce += WanderForce;

            mAvoid.UpdateBehaviour();
            totalForce += AvoidWallsForce;

        }

//        totalForce /= mBody.mass;
        Vector3 steeringForce = totalForce - new Vector3(mBody.velocity.x, 0, mBody.velocity.z);

//        steeringForce = Vector3.ClampMagnitude(steeringForce, MaxSpeed);

        mBody.velocity = mBody.velocity + steeringForce + (gravity * Time.fixedDeltaTime);

        velocityV = mBody.velocity;
    }

    public Vector3 velocityV;  // TODO Temporary variable to display velocity
}
