﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{ 
    public delegate void GameEvent();
    public static event GameEvent OnDecontamination;
    public static event GameEvent OnExpload;
    public static event GameEvent OnEscape;

    public enum BehaviourState { Disabled, Wandering, Hunting }
    private BehaviourState mState;

    static public int ActiveAgentsCount { get; private set; }  // Keep track of how many of the preloaded agents are enabled

    static public float ColliderRadius { get; private set; }  // Used to check whether it is safe to spawn

    static private WaitForSeconds waitForDecisionTick = new WaitForSeconds(0.3f); // How long to wait between checking the AI decision loop
    static private WaitForSeconds waitForExplosionCountDownTime = new WaitForSeconds(3f);  // How long to wait before exploding
    static private WaitForSeconds waitForLeaderTrailTick = new WaitForSeconds(1f);  // How long to wait between logging new leader trail positions

    static private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    static private GameObject agentPrefab;  // Remove once randomised function written

    public RaycastHit [] hits { get; private set; }  // A record of the most recent objects hit by the decision check

    public Vector3 FlockForce       { get; set; }
    public Vector3 WanderForce      { get; set; }
    public Vector3 AvoidWallsForce  { get; set; }
    public Vector3 HuntForce        { get; set; }

    private HuntPlayer mHunt;
    private FlockWithGroup mFlock;
    private Wander mWander;
    private AvoidWalls mAvoid;

    public bool isLeader { get; private set; }
    private float leaderCheckRadius = 14;
    public Agent leaderToFollow;  //TODO change to private
    private Vector3 [] trail;
    public Tile currentTile;  // TODO change to private
    public Tile targetTile;  // TODO Change to private
    public Tile previousTile;  // TODO Change to private
    private float resetTargetChoiceTime = 4f;  // How long to wait before selecting a new destination tile
    private float resetTargetChoiceTimer;

    private Rigidbody mBody;

    private Vector3 gravity;  // Keep a constant value for gravity to avoid recalculating each time
    [SerializeField]
    private float Speed;

    private bool shouldExpload;  // 

    public Vector3 VVelocity; // TODO remove temporary for visualising velocity
    public Vector3 VSeekForce;

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

        trail = new Vector3[5];

        mHunt = GetComponent<HuntPlayer>();
        mFlock = GetComponent<FlockWithGroup>();
        mWander = GetComponent<Wander>();
        mAvoid = GetComponent<AvoidWalls>();

        ActiveAgentsCount++;
        Reset();
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

        if (ActiveAgentsCount == 1)
        {
            isLeader = true;
        }

        currentTile = Map.Get.AgentSpawnTile;
        targetTile = currentTile;

        // Place agent on spawn point
        mBody.MovePosition(Map.Get.AgentSpawnLocation);
        mBody.MoveRotation(Quaternion.identity);

        mState = BehaviourState.Wandering;  // Set agent to wander state

        StartCoroutine(Co_DecisionTick());  // Run behaviour machine
    }

    public void Decontaminate()
    {
        // Playsound
        Reset();
        
        if(OnDecontamination != null)
        {
            OnDecontamination();
        }
    }

    public void ArriveAtTile(Tile tileArrivedAt)
    {
        currentTile = tileArrivedAt;
    }

    // Coroutiene behaviour machine, handling global awareness and state switching
    private IEnumerator Co_DecisionTick()
    {
        while (mState != BehaviourState.Disabled)
        {
            if (!isLeader)
            {
                if ( leaderToFollow == null
                  || (leaderToFollow.transform.position - transform.position).sqrMagnitude > leaderCheckRadius * leaderCheckRadius)
                {
                    if (!FindALeader())
                    {
                        // If no valid leader then become one
                        isLeader = true;
                        leaderToFollow = null;
                        StartCoroutine(Co_LeaderTrailTick());
                        StartCoroutine(Co_ResetTargetChoiceTimer());
                    }
                }
            }
            else
            {
                // Can I see the player
                if (IsTargetVisible(Player.Get.gameObject))
                {
                    // Hunt the player
                    print("I CAN SEE THE PLAYER... CHARGE!!!");
                }
                else if (currentTile == targetTile)
                {
                    // What are the avilable exits from this tile
                    int numberOfAvailableExits = currentTile.ExitTiles.Length;
                    if ( numberOfAvailableExits == 0
                      && !shouldExpload
                      && currentTile != Map.Get.AgentSpawnTile)
                    {
                        // If there are no exits the leader should start timer on exploading (from the stress of not knowing where to go!)
                        StartCoroutine(Co_PrepareToExpload());
                    }
                    else if (numberOfAvailableExits == 1)
                    {
                        // Only going backwards is available so go back to previous tile
                        targetTile = currentTile.ExitTiles[0];
                        previousTile = currentTile;
                        resetTargetChoiceTimer = Time.fixedTime + resetTargetChoiceTime;
                    }
                    else if (numberOfAvailableExits == 2)
                    {
                        // Only one new tile is available so choose that one
                        for (int i = 0; i < numberOfAvailableExits; i++)
                        {
                            if (currentTile.ExitTiles[i] != previousTile)
                            {
                                targetTile = currentTile.ExitTiles[i];
                                previousTile = currentTile;
                                resetTargetChoiceTimer = Time.fixedTime + resetTargetChoiceTime;
                                break;
                            }
                        }
                    }
                    else if (numberOfAvailableExits > 2)
                    {
                        // Choose a random exit, excluding the previous tile visited
                        int randomExitIndex = 0;
                        do
                        {
                            randomExitIndex = Random.Range(0, numberOfAvailableExits);
                        } while (currentTile.ExitTiles[randomExitIndex] == previousTile);
                        targetTile = currentTile.ExitTiles[randomExitIndex];
                        previousTile = currentTile;
                        resetTargetChoiceTimer = Time.fixedTime + resetTargetChoiceTime;
                    }

                    if ( shouldExpload
                      && currentTile != targetTile)
                    {
                        // If a path is now available cancel any impending explosions
                        shouldExpload = false;
                    }
                }
            }

//            currentTile = WhichTileIsBelowMe();


//            for (int i = 0; i < 16; i++)
//            {
//                Vector3 rayDirection = new Vector3(Mathf.Cos(22.5f * i), 0, Mathf.Sin(22.5f * i));
//                Physics.Raycast(transform.position + Vector3.up, rayDirection, out hits[i]);
//                Debug.DrawRay(transform.position + Vector3.up, rayDirection, Color.blue, decisionTickTime);
//            }
//
//            if (HuntForce.sqrMagnitude > 0)
//            {
//                mState = BehaviourState.Hunting;
//            }
//            else
//            {
//                mState = BehaviourState.Wandering;
//            }

            yield return waitForDecisionTick;
        }
    }

    void FixedUpdate()
    {
        if (mState != BehaviourState.Disabled)
        {
            Vector3 targetPos;
            if (!isLeader)
            {
                // Follow the leader
                targetPos = leaderToFollow.WhereShouldIGo(transform.position);
                // Move in direction of target
            }
            else
            {
                // Wander towards target tile
                targetPos = targetTile.transform.position;
            }
            Vector3 seekForce = (-transform.position + targetPos).normalized;
            seekForce *= Speed;

            mBody.velocity = mBody.velocity + seekForce + (gravity * Time.fixedDeltaTime);

            VSeekForce = seekForce;
            VVelocity = mBody.velocity;


//        // Add forces according to desision state
//        Vector3 totalForce = Vector3.zero;
//
//        mHunt.UpdateBehaviour();  // Check if player has been seen and calculate steering force contribution
//        totalForce += HuntForce;
//
//        if (mState == BehaviourState.Wandering)
//        {
//            totalForce += FlockForce;
//
//            totalForce += WanderForce;
//
//            mAvoid.UpdateBehaviour();
//            totalForce += AvoidWallsForce;
//        }
//
//        Vector3 steeringForce = totalForce - new Vector3(mBody.velocity.x, 0, mBody.velocity.z);
//
////        steeringForce = Vector3.ClampMagnitude(steeringForce, MaxSpeed);
//
//        mBody.velocity = mBody.velocity + steeringForce + (gravity * Time.fixedDeltaTime);
        }
    }

    private IEnumerator Co_LeaderTrailTick()
    {
        while (isLeader)
        {
            // Update the trail
            for (int i = 0; i < 4; i++)
            {
                // Move down the older locations towards the tail, removing the oldest
                trail[i] = trail[i+1];
            }
            // Add the current location to the newest slot in the trail
            trail[4] = transform.position;

            yield return waitForLeaderTrailTick;
        }
    }

    // Target reset timeout for solving interuptions from wall collision or tiles being slid
    private IEnumerator Co_ResetTargetChoiceTimer()
    {
        while (mState != BehaviourState.Disabled)
        {
            resetTargetChoiceTimer = Time.fixedTime + resetTargetChoiceTime;
            while (Time.fixedTime < resetTargetChoiceTimer)
            {
                yield return waitForFixedUpdate;
            }

            // If timer runs out return to current tile and choose new destination
            targetTile = currentTile;
            yield return waitForFixedUpdate;
        }
    }

    // Check for a Leader to follow
    private bool FindALeader()
    {
        for (int i = 0; i < ActiveAgentsCount; i++)
        {
            Agent potentialLeader = GameManager.Get.mAgents[i];
            // Check all agents for being within valid proximity
            // Are any of them a leader
            // Is that leader reachable
            if ( (potentialLeader.transform.position - transform.position).sqrMagnitude <= leaderCheckRadius * leaderCheckRadius
              && potentialLeader.isLeader
              && IsTargetVisible(potentialLeader.gameObject))
            {
                // Follow that leader
                leaderToFollow = potentialLeader;
                return true;
            }
        }
        // Or if there is no valid leader
        return false;
    }

    // Cast a ray at the target and check whether there is another object in the way
    private bool IsTargetVisible(GameObject target)
    {
        Vector3 differenceVector = target.transform.position - transform.position;
        RaycastHit hit;
        Physics.Raycast(transform.position, differenceVector.normalized, out hit, differenceVector.magnitude);
        Debug.DrawLine(transform.position, target.transform.position, Color.red, 2f);
        if (hit.collider.gameObject == target)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Vector3 WhereShouldIGo(Vector3 followerPosition)
    {
        return trail[0];  // TODO check follower location against trail in case they are closer to the leader than the back of the trail and return the next closest
    }

    private Tile WhichTileIsBelowMe()
    {
        RaycastHit belowMe;
        Physics.Raycast(transform.position + Vector3.up, Vector3.down, out belowMe);
        return belowMe.collider.GetComponentInParent<Tile>();
    }

    private IEnumerator Co_PrepareToExpload()
    {
        shouldExpload = true;

        // TODO Warn player about to expload
        // TODO Playsound "Oh no!"
        print("OH NO!!!");
        // TODO Scale up
        transform.localScale = Vector3.one * 3;

        yield return waitForExplosionCountDownTime;
        if (shouldExpload)
        {
            Expload();
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    private void Expload()
    {
        // TODO Playsound "pop!"
        if(OnExpload != null)
        {
            OnExpload();
        }
        Reset();
    }
}
