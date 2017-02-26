using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{ 
    static public int ActiveAgentsCount { get; private set; }

    static public float ColliderRadius { get; private set; }

    static private float desisionTickTime = 0.3f;
    static private WaitForSeconds waitForDesisionTick = new WaitForSeconds(desisionTickTime);

    static private GameObject agentPrefab;  // Remove once randomised function written

    private Vector3 gravity;

    public Vector3 FlockVector;  //{ get; set; }
    public Vector3 HuntVector;   //{ get; set; }
    public Vector3 WanderVector; //{ get; set; }
    public Vector3 locomoationForce;  // TODO set back to private

    [SerializeField]
    private float Speed = 500;

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
        ColliderRadius = GetComponent<CapsuleCollider>().radius;

        ActiveAgentsCount++;
        Reset();
    }

    void Start()
    {
        gravity = Vector3.down * GameManager.Get.gravity * mBody.mass;
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        ActiveAgentsCount--;
        transform.position = Vector3.down * 100;
        FlockVector = Vector3.zero;
        HuntVector = Vector3.zero;
        locomoationForce = Vector3.zero;
    }

    public void Init()
    {
        gameObject.SetActive(true);
        ActiveAgentsCount++;
        transform.position = Map.Get.AgentSpawnLocation;
        transform.rotation = Quaternion.identity;
        StartCoroutine(Co_DesisionTick());
    }

    private IEnumerator Co_DesisionTick()
    {
        while (gameObject.activeSelf)
        {
            // Use context map to decide on weighting of heading
            locomoationForce = Vector3.zero;
           // locomoationForce += FlockVector + HuntVector;
            locomoationForce += WanderVector;
            locomoationForce *= mBody.mass;
            locomoationForce *= Speed * Time.fixedDeltaTime;
            yield return waitForDesisionTick;
        }
    }

    void FixedUpdate()
    {
        mBody.AddForce(locomoationForce + gravity);
//        mBody.velocity = new Vector3(mBody.velocity.x, Mathf.Clamp(mBody.velocity.y, -9.8f, 9.8f), mBody.velocity.z);

        velocityV = mBody.velocity;
    }

    public Vector3 velocityV;  // TODO Temporary variable to display velocity
}
