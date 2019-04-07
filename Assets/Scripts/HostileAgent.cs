using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HostileAgent : MonoBehaviour
{
    [SerializeField]
    Transform _destination;
    NavMeshAgent _navMeshAgent;
    public int remainingAgents;
    public int agentsDestroyed;

    int rndFindHiding;

    int speed;

    public int damage;
    //int range;

    bool hasBeenHit;

    //find target variables
    GameObject[] gos;
    public GameObject closest;
    public GameObject secClosest;
    public GameObject notClosest;

    // Start is called before the first frame update
    void Start()
    {
        //you could do a method to spawn agents
        //i didnt due to how i was locating the nearest agent

        //use navmesh and select first target
        _navMeshAgent = this.GetComponent<NavMeshAgent>();
        if (_navMeshAgent == null)
        {
            Debug.Log("you aint attached the navmesh dummy");
        }
        else
        {
            SetDestination();
        }

        //set values
        speed = 5; //moves faster than base agents
        //damage = 35; //will take 3 hits to defeat a normal agent
        
        hasBeenHit = false; //if a normal agent hits, then slow by 50%
    }

    private void SetDestination()
    {

        //find closest agent set that as target
        gos = GameObject.FindGameObjectsWithTag("Target");
        
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            float distance2;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }

            else if (curDistance > distance)
            {
                notClosest = go;
                distance2 = curDistance;
            }
            else
            {
                secClosest = go;
            }
        }

        Vector3 targetDest = closest.transform.position;
        _navMeshAgent.SetDestination(targetDest);

    }

    // Update is called once per frame
    void Update()
    {
        rndFindHiding = 0;
        SetDestination();
        NormalAgent normalAgent = closest.GetComponent<NormalAgent>();

        if(normalAgent.hiding == true)
        {

            rndFindHiding = Random.Range(0, 2);
            if (rndFindHiding >= 1)
            {
                Vector3 targetDest = notClosest.transform.position;
                _navMeshAgent.SetDestination(targetDest);
            }
        }
    }
}
