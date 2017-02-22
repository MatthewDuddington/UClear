using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum State { Paused, Playing }

    [SerializeField]
    private GameObject [] SpawnPrefabs;

    [SerializeField]
    private Player PlayerPrefab;

    [SerializeField]
    private Arena Arena;

    [SerializeField]
    private float TimeBetweenSpawns;

    [SerializeField]
    private int numberOfAgents = 30;

    private Agent [] mAgents;  // Changed mObjects List to mAgents Array to reflect its use in this game and known size
    private Player mPlayer;
    private State mState;
//    private float mNextSpawn;

    void Awake()
    {
        mPlayer = Instantiate(PlayerPrefab);
        mPlayer.transform.parent = transform;

        ScreenManager.OnNewGame += ScreenManager_OnNewGame;
        ScreenManager.OnExitGame += ScreenManager_OnExitGame;

        mAgents = new Agent[numberOfAgents];
    }

    void Start()
    {
        Arena.Calculate();
        PopulateAgents();
//        mPlayer.enabled = false;
//        mState = State.Paused;
        mPlayer.enabled = true;
        mState = State.Playing;
    }

    // Changed ticking random spawn in Update function to instead pre-load a list of random agents
    private void PopulateAgents()
    {
        for (int i = 0; i < numberOfAgents; i++)
        {
            GameObject spawnAgent = Agent.GenerateRandomAgentDesign();
            GameObject spawnedInstance = Instantiate(spawnAgent, Map.Get.AgentSpawnLocation, Quaternion.identity);
            Agent spawnedAgent = spawnedInstance.gameObject.GetComponent<Agent>();
            mAgents[i] = spawnedAgent;
            mAgents[i].Reset();
        }
    }

    private void SpawnAgent()
    {
        mAgents[Agent.ActiveAgentsCount].Init();
    }

    private void BeginNewGame()
    {
        for (int count = 0; count < mAgents.Length; ++count)
        {
            mAgents[count].Reset();  // Disable and reset rather than Destroy to avoid realocating memeory
        }

        mPlayer.transform.position = new Vector3(0.0f, 0.5f, 0.0f);
//        mNextSpawn = TimeBetweenSpawns;
        mPlayer.enabled = true;
        mState = State.Playing;
    }

    private void EndGame()
    {
        mPlayer.enabled = false;
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
}
