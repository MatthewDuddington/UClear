using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void GameEvent();
    public static event GameEvent OnRadiationDamage;

    // Easy accessor for the class instance
    private static GameManager gameManager_;
    public static GameManager Get
    { 
        get
        {
            if (gameManager_ == null)
            {
                Debug.LogError("No GameManager present in scene");
            }
            return gameManager_;
        }
        private set { gameManager_ = value; }
    } 

    public enum State { Paused, Playing }

    public float gravity = 9.8f;

    public AnimationCurve liftCurve;  // Curve to drive height of tiles when lifted off the map

    [SerializeField]
    private GameObject [] SpawnPrefabs;

    [SerializeField]
    private Player PlayerPrefab;

    [SerializeField]
    private Arena Arena;

    [SerializeField]
    private float TimeBetweenSpawns;
    private WaitForSeconds waitForTimeBetweenSpawns;

    [SerializeField]
    private float RadiationTickTime;
    private WaitForSeconds waitForRadiationTick;

    [SerializeField]
    private int numberOfAgentsToSpawn = 30;

    public int numberOfResearchersToWin = 10;

    private int radiationLevel = 0;

    private Agent [] mAgents;  // Changed mObjects List to mAgents Array to reflect its use in this game and known size
    private Player mPlayer;
    private State mState;
    private float mNextSpawn;


    void Awake()
    {
        gameManager_ = this;

        mPlayer = Instantiate(PlayerPrefab);
        mPlayer.transform.parent = transform;

        ScreenManager.OnNewGame += ScreenManager_OnNewGame;
        ScreenManager.OnExitGame += ScreenManager_OnExitGame;

        mAgents = new Agent[numberOfAgentsToSpawn];

        waitForTimeBetweenSpawns = new WaitForSeconds(TimeBetweenSpawns);
        waitForRadiationTick = new WaitForSeconds(RadiationTickTime);
    }

    void Start()
    {
//        Arena.Calculate();
        PreLoadAgents();
//        mPlayer.enabled = false;
//        mState = State.Paused;
        mPlayer.gameObject.SetActive(true);
        mState = State.Playing;
        StartCoroutine(Co_SpawnAgents());
    }

    // Changed ticking random spawn in Update function to instead pre-load a list of random agents
    private void PreLoadAgents()
    {
        for (int i = 0; i < numberOfAgentsToSpawn; i++)
        {
            GameObject spawnAgent = Agent.GenerateRandomAgentDesign();
            GameObject spawnedInstance = Instantiate(spawnAgent, Vector3.down * 100, Quaternion.identity);
            Agent spawnedAgent = spawnedInstance.gameObject.GetComponent<Agent>();
            mAgents[i] = spawnedAgent;
        }
    }

    private IEnumerator Co_SpawnAgents()
    {
        while (mState == State.Playing && Agent.ActiveAgentsCount < numberOfAgentsToSpawn)
        {
            // Check whether spawning an agent would cause it to overlap with an existing agent
            bool shouldSpawn = true;
            Collider[] spawnCollisions = Physics.OverlapSphere(Map.Get.AgentSpawnLocation, Agent.ColliderRadius);
            foreach (Collider other in spawnCollisions)
            {
                if (other.CompareTag("Boffin"))
                {
                    shouldSpawn = false;
                }
            }
            // If spawn location is clear, then spawn the agent
            if (shouldSpawn)
            {
                mAgents[Agent.ActiveAgentsCount].Init();
            }
            yield return waitForTimeBetweenSpawns;
        }
    }

    private void BeginNewGame()
    {
        for (int count = 0; count < mAgents.Length; ++count)
        {
            mAgents[count].Reset();  // Disable and reset rather than Destroy to avoid realocating memeory
        }

        mPlayer.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        mNextSpawn = TimeBetweenSpawns;
        mPlayer.gameObject.SetActive(true);
        mState = State.Playing;
    }

    private void EndGame()
    {
        mPlayer.gameObject.SetActive(false);
        mState = State.Paused;
    }

    private void ScreenManager_OnNewGame()
    {
        BeginNewGame();
    }

    private void ScreenManager_OnExitGame()
    {
        EndGame();
    }

    public void AddRadiation(int radiationAmount)
    {

    }

    private IEnumerator RadiationTick()
    {
        while (State.Playing)
        {
            yield return waitForRadiationTick;
            AddRadiation(1);
        }
    }
}
