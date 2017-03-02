using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{ 
    public delegate void GameEvent();
    public static event GameEvent OnDecontamination;

    public enum BehaviourState { Disabled, Wandering, Hunting }
    private BehaviourState mState;

    static public int ActiveAgentsCount { get; private set; }  // Keep track of how many of the preloaded agents are enabled

    static public float ColliderRadius { get; private set; }  // Used to check whether it is safe to spawn

    static private WaitForSeconds waitForDecisionTick = new WaitForSeconds(0.3f); // How long to wait between checking the AI decision loop
    static private WaitForSeconds waitForExplosionCountDownTime = new WaitForSeconds(3f);  // How long to wait before exploding
    static private WaitForSeconds waitForLeaderTrailTick = new WaitForSeconds(1f);  // How long to wait between logging new leader trail positions

    static private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    static private GameObject agentPrefab0;  // Remove once randomised function written
    static private GameObject agentPrefab1;

    public RaycastHit [] hits { get; private set; }  // A record of the most recent objects hit by the decision check

    // Flock, Wander, Avoid and Hunt no longer used in this version as behaviour was not as desired yet
    // Needed to ship a functioning AI for the game to be playable, so an alternative to my original approach has been used for now
    // Sorry that this bypasses some of the most interesting starter code! But the overall game idea was still inspired by it
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
    private bool priorityTarget = false;
    private float resetTargetChoiceTime = 4f;  // How long to wait before selecting a new destination tile
    private float resetTargetChoiceTimer;

    private Rigidbody mBody;

    private Vector3 gravity;  // Keep a constant value for gravity to avoid recalculating each time
    [SerializeField]
    private float Speed;
    private float originalSpeed = 0;

    private bool shouldExpload;  // 

    public Vector3 VVelocity; // TODO remove temporary for visualising velocity
    public Vector3 VSeekForce;

    // TODO Load in different agent bodyparts and return a randomised character
    static public GameObject GenerateRandomAgentDesign()
    {
        GameObject randomAgent;
        if (agentPrefab0 == null)
        {
            agentPrefab0 = Resources.Load<GameObject>("Boffin_F");
            agentPrefab1 = Resources.Load<GameObject>("Boffin_M");   
        }

        if (Random.Range(0,2) == 0)
        {
            randomAgent = agentPrefab0;
        }
        else
        {
            randomAgent = agentPrefab1;
        }

        return randomAgent;
    }

    void Awake()
    {
        mBody = gameObject.GetComponent<Rigidbody>();
        ColliderRadius = GetComponent<CapsuleCollider>().radius;
        hits = new RaycastHit[16];

        trail = new Vector3[5];

        if (originalSpeed == 0)  // Avoid overriding with faster speed while agents are still spawning
        {
            originalSpeed = Speed;
        }

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
//        FlockForce = Vector3.zero;
//        WanderForce = Vector3.zero;
//        AvoidWallsForce = Vector3.zero;
//        HuntForce = Vector3.zero;

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
        previousTile = currentTile;
        targetTile = currentTile;

        // Place agent on spawn point
        mBody.MovePosition(Map.Get.AgentSpawnLocation);
        mBody.MoveRotation(Quaternion.identity);

        mState = BehaviourState.Wandering;  // Set agent to wander state

        StartCoroutine(Co_DecisionTick());  // Run behaviour machine
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            if (other.GetComponent<Tile>().myTileType == Tile.TileType.Decontam)
            {
                Decontaminate();
            }
            else if (other.GetComponent<Tile>().myTileType == Tile.TileType.Escape)
            {
                Escape();
            }
            else
            {
                ArriveAtTile(other.GetComponent<Tile>());
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Expload();
        }
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
                priorityTarget = false;

                // Can I see the player
                if (IsTargetVisible(Player.Get.gameObject))
                {
                    // Hunt the player
                    print("I CAN SEE THE PLAYER... CHARGE!!!");
                    priorityTarget = true;
                }

                // Can I see an escape exit
                else if (currentTile.Index.Col % 2 != 0)
                {
                    if (currentTile.Index.Row == 0)  // North edge
                    {
                        GameObject targetEscapeTile = Map.Get.EscapeTiles[(int) ((currentTile.Index.Col - 1) * 0.5f)].gameObject;
                        if (IsTargetVisible(targetEscapeTile))
                        {
                            print("Can see escape exit north");
                            targetTile = targetEscapeTile.GetComponent<Tile>();
                            priorityTarget = true;
                        }
                    }
                    else if (currentTile.Index.Row == Map.Get.MapSizeVertical - 1)  // South edge
                    {
                        GameObject targetEscapeTile = Map.Get.EscapeTiles[(int) ((((Map.Get.MapSizeHorizontal - 1) * 0.5f) - 1)
                                                                         +      (((Map.Get.MapSizeVertical - 1) * 0.5f))
                                                                         +      ((currentTile.Index.Col - 1) * 0.5f)) ].gameObject;
                        if (IsTargetVisible(targetEscapeTile))
                        {
                            print("Can see escape exit south");
                            targetTile = targetEscapeTile.GetComponent<Tile>();
                            priorityTarget = true;
                        }
                    }
                }
                else if (currentTile.Index.Row % 2 != 0)
                {
                    if (currentTile.Index.Col == Map.Get.MapSizeHorizontal - 1)  // East edge
                    {
                        GameObject targetEscapeTile = Map.Get.EscapeTiles[(int) ((((Map.Get.MapSizeHorizontal - 1) * 0.5f) - 1)
                                                                         +      ((currentTile.Index.Row - 1) * 0.5f)) ].gameObject;
                        if (IsTargetVisible(targetEscapeTile))
                        {
                            print("Can see escape exit east");
                            targetTile = targetEscapeTile.GetComponent<Tile>();
                            priorityTarget = true;
                        }
                    }
                    else if (currentTile.Index.Col == 0)  // West edge
                    {
                        GameObject targetEscapeTile = Map.Get.EscapeTiles[(int) (((((Map.Get.MapSizeHorizontal - 1) * 0.5f) * 2) - 1)
                                                                         +      ((Map.Get.MapSizeVertical - 1) * 0.5f)
                                                                         +      ((currentTile.Index.Row - 1) * 0.5f)) ].gameObject;
                        if (IsTargetVisible(targetEscapeTile))
                        {
                            print("Can see escape exit west");
                            targetTile = targetEscapeTile.GetComponent<Tile>();
                            priorityTarget = true;
                        }
                    }
                }

                // If there is no player or escape exit in view, check for a regular tile 
                if ( !priorityTarget
                  && currentTile == targetTile)
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

                    // If a path is now available cancel any impending explosions
                    if ( shouldExpload
                      && currentTile != targetTile)
                    {
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
            Speed = originalSpeed;

            Vector3 targetPos = Vector3.zero;
            if (!isLeader)
            {
                // Follow the leader
                targetPos = leaderToFollow.WhereShouldIGo(transform.position);
                // Move in direction of target
            }
            else if (priorityTarget)
            {
                Speed = originalSpeed * 1.03f;
                if (targetTile.myTileType == Tile.TileType.Escape)
                {
                    targetPos = targetTile.transform.position;
                }
                else
                {
                    targetPos = GameManager.Get.mPlayer.transform.position;
                }
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
        if ( hit.collider
          && hit.collider.gameObject == target)
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

        transform.localScale = Vector3.one * 3;

        switch (Random.Range(0,4))
        {
            case 0:
            {
                GameManager.Get.audio.PlayOneShot(GameManager.OhNo1);
                break;
            }
            case 1:
            {
                GameManager.Get.audio.PlayOneShot(GameManager.OhNo2);
                break;
            }
            case 2:
            {
                GameManager.Get.audio.PlayOneShot(GameManager.OhNo3);
                break;
            }
            case 3:
            {
                GameManager.Get.audio.PlayOneShot(GameManager.OhNo4);
                break;
            }
        }

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
        GameManager.Get.audio.PlayOneShot(GameManager.PopSplat);
        GameManager.Get.RadiationDamage(GameManager.Get.RadiationDamageFromAgents);
        Reset();
    }

    public void Escape()
    {
        GameManager.Get.RadiationDamage(GameManager.Get.RadiationDamageFromAgents);
        // TODO Playsound "Wheee!"
        Reset();
    }
}
