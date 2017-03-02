using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void GameEvent(float value);
    public static event GameEvent OnRadiationDamage;

    // Easy accessor for the class instance
    private static GameManager This;
    public static GameManager Get
    { 
        get
        {
            if (This == null)
            {
                Debug.LogError("No GameManager present in scene");
            }
            return This;
        }
        private set { This = value; }
    } 

    public enum State { Paused, Playing, GameOver }

    public float gravity { get { return 9.8f; } }

    public AnimationCurve liftCurve;  // Curve to drive height of tiles when lifted off the map

    [SerializeField]
    private GameObject [] SpawnPrefabs;

    [SerializeField]
    private Player PlayerPrefab;

    [SerializeField]
    private Arena Arena;

    [SerializeField]
    private float TimeBetweenSpawns = 1;  // 1
    private WaitForSeconds waitForTimeBetweenSpawns;

    [SerializeField]
    private float RadiationTickTime = 1;  // 1
    private WaitForSeconds waitForRadiationTick;

    [SerializeField]
    private int  NumberOfAgentsToSpawn = 15;                          // 15
    public int   NumberOfDecontamToWin     { get { return 10; } }     // 10
    public float AmbiantRadiationDamage    { get { return 0.05f; } }  // 0.05f
    public float RadiationDamageFromAgents { get { return 0.1f; } }   // 0.1f

    public Agent [] mAgents { get; private set; }  // Changed mObjects <List> to mAgents [Array] to reflect its use in this game and known size
    public State GameState  { get; private set; }  // Changed private mState to public GameState to enable ticking objects to check for pauseing

    private Player mPlayer;



    void Awake()
    {
        This = this;

        mPlayer = Instantiate(PlayerPrefab);
        mPlayer.transform.parent = transform;

        ScreenManager.OnNewGame += ScreenManager_OnNewGame;
        ScreenManager.OnExitGame += ScreenManager_OnExitGame;

        mAgents = new Agent[NumberOfAgentsToSpawn];

        waitForTimeBetweenSpawns = new WaitForSeconds(TimeBetweenSpawns);
        waitForRadiationTick = new WaitForSeconds(RadiationTickTime);
    }

    void Start()
    {
//        Arena.Calculate();
        PreLoadAgents();
        mPlayer.gameObject.SetActive(false);
        GameState = State.Paused;
//        mPlayer.gameObject.SetActive(true);
//        mState = State.Playing;
//        StartCoroutine(Co_SpawnAgents());
    }

    private void BeginNewGame()
    {
        for (int count = 0; count < Agent.ActiveAgentsCount; ++count)
        {
            mAgents[count].Reset();  // Disable and reset rather than Destroy to avoid realocating memeory
        }

        Map.Get.GenerateNewMap();

        mPlayer.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
        GameState = State.Playing;
        mPlayer.gameObject.SetActive(true);
        StartCoroutine(Co_SpawnAgents());
        StartCoroutine(Co_RadiationTick());
    }

    private void PauseGame()
    {
        mPlayer.gameObject.SetActive(false);
        GameState = State.Paused;
    }

    private void ResumeGame()
    {
        mPlayer.gameObject.SetActive(true);
        GameState = State.Playing;
    }

    private void EndGame()
    {
        mPlayer.gameObject.SetActive(false);
        GameState = State.GameOver;
    }

    // When subscribed events are triggered in ScreenManager, Begin or End the game
    private void ScreenManager_OnNewGame()
    {
        BeginNewGame();
    }

    private void ScreenManager_OnExitGame()
    {
        EndGame();
    }
    
    // Changed ticking random spawn in Update function to instead pre-load a list of random agents
    private void PreLoadAgents()
    {
        for (int i = 0; i < NumberOfAgentsToSpawn; i++)
        {
            GameObject spawnAgent = Agent.GenerateRandomAgentDesign();
            GameObject spawnedInstance = Instantiate(spawnAgent, Vector3.down * 100, Quaternion.identity);
            Agent spawnedAgent = spawnedInstance.gameObject.GetComponent<Agent>();
            spawnedAgent.transform.SetParent(transform);
            mAgents[i] = spawnedAgent;
        }
    }
    
    private IEnumerator Co_SpawnAgents()
    {
        while (GameState == State.Playing && Agent.ActiveAgentsCount < NumberOfAgentsToSpawn)
        {
            // Check whether spawning an agent would cause it to overlap with an existing agent
            bool shouldSpawn = true;
            Collider[] spawnCollisions = Physics.OverlapSphere(Map.Get.AgentSpawnLocation, Agent.ColliderRadius);
            foreach (Collider other in spawnCollisions)
            {
                if (other.CompareTag("Agent"))
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

    public void RadiationDamage(float radiationAmount)
    {
        if (OnRadiationDamage != null)
        {
            OnRadiationDamage(radiationAmount);
        }
    }

    private IEnumerator Co_RadiationTick()
    {
        while (GameState == State.Playing)
        {
            yield return waitForRadiationTick;
            RadiationDamage(0.005f);
        }
    }
}
