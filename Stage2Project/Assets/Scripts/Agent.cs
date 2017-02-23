using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{ 
    static public int ActiveAgentsCount { get; private set; }

    static private float desisionTickTime = 0.3f;
    static private WaitForSeconds waitForDesisionTick = new WaitForSeconds(desisionTickTime);

    static private GameObject agentPrefab;  // Remove once randomised function written

    public Vector3 FlockForce { get; set; }
    public Vector3 HuntForce  { get; set; }
    private Vector3 locomoationForce;

    private Rigidbody mBody;

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

        ActiveAgentsCount++;
        Reset();
    }

    public void Reset()
    {
        enabled = false;
        ActiveAgentsCount--;
        transform.position = Vector3.down * 100;
        FlockForce = Vector3.zero;
        HuntForce = Vector3.zero;
        locomoationForce = Vector3.zero;
    }

    public void Init()
    {
        enabled = true;
        ActiveAgentsCount++;
        transform.position = Map.Get.AgentSpawnLocation;
        transform.rotation = Quaternion.identity;
        StartCoroutine(Co_DesisionTick());
    }

    private IEnumerator Co_DesisionTick()
    {
        while (enabled)
        {
            // Use context map to decide on weighting of heading
            locomoationForce = FlockForce + HuntForce;
            yield return waitForDesisionTick;
        }
    }

    void FixedUpdate()
    {
        mBody.AddForce(locomoationForce);
    }
}
