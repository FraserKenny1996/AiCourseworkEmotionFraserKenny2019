using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NormalAgent : MonoBehaviour
{
    //emotional values
    int fear;
    int happiness;
    int safe;
    int helplesness;
    int adrenaline;

    //physical condition values
    int health;
    float speed;

    public float dangerRange = 2f;
    public float damageRange = 1f;

    [SerializeField]
    bool patrolWaiting;

    [SerializeField]
    float totalWaitTime = 3f;

    [SerializeField]
    float switchProbability = 0.2f;

    [SerializeField]
    public GameObject[] patrolPoints;

    [SerializeField]
    Transform _destination;

    public GameObject escapePoint;

    NavMeshAgent navMeshAgent;
    int currentPatrolIndex;
    bool traveling;
    bool waiting;
    bool patrolForward;
    float waitTimer;

    bool damaged;
    bool seenDamage;
    public bool hiding;
    bool escaped;
    bool trapped;
    public bool escaping;

    GameObject hostile;

    public Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        health = 100;
        speed = 3.5f;

        fear = 0;
        happiness = 100;
        safe = 100;
        helplesness = 0;
        adrenaline = 0;

        damaged = false;
        seenDamage = false;
        hiding = false;
        escaped = false;
        trapped = false;
        escaping = false;

        navMeshAgent = this.GetComponent<NavMeshAgent>();


        rend = GetComponent<Renderer>();

        if (navMeshAgent == null)
        {
            Debug.Log("you aint attached the navmesh dummy");
        }
        else
        {
            if(patrolPoints != null && patrolPoints.Length >= 2)
            {
                currentPatrolIndex = 0;
                ChangePatrolPoint();
                SetDestination();
            }
            else
            {
                Debug.Log("you no set up nodes mang");
            }
        }
    }

    // Update is called once per frame
    public void Update()
    {
        if(traveling && navMeshAgent.remainingDistance <= 0.1f)
        {
            traveling = false;

            if (hiding == true)
            {
                escaping = false;
                waiting = true;
                waitTimer = 0.0f;
                ChangePatrolPoint();
            }   
            else
            {
                SetDestination();
            }
        }

        if(waiting)
        {
            waitTimer += Time.deltaTime;
            if(waitTimer >= totalWaitTime)
            {
                waiting = false;

                ChangePatrolPoint();
            }

        }
        if (Vector3.Distance(this.transform.position, escapePoint.transform.position) <= 0.5f)
        {
            escaped = true;
        }

        //effect of not escaped
        if (!escaped)
        {
            safe -= 1;
            happiness--;
        }

       

        //check if hostile is close
        Danger();

        //check if adrenaline went up
        AdrenalineIncreased();
        //decrease adrenaline over time
        if (adrenaline > 0)
        {
            adrenaline--;
        }

        //check remaining health
        CheckHp();

        //check if feeling safe
        FeelingSafe();

        if(hiding)
        {
            rend.material.color += new Color( 0.0f, 0.0f, 1.0f);
            safe += 40;
        }
        if (damaged)
        {
            rend.material.color += new Color(1.0f, 0.0f, 0.0f);
        }

        //set hiding and escaping
        if (safe > 50)
        {
            hiding = false;
            escaping = true;
        }
    }

    private void SetDestination()
    {
        if (patrolPoints != null & hiding == true)
        {
            Vector3 targetVetor = patrolPoints[currentPatrolIndex].transform.position;
            navMeshAgent.SetDestination(targetVetor);
            traveling = true;
        }
        if (patrolPoints != null & hiding == false)
        {
            Vector3 targetVetor = escapePoint.transform.position;
            navMeshAgent.SetDestination(targetVetor);
            traveling = true; 
        }
    }

    private void ChangePatrolPoint()
    {
        if(UnityEngine.Random.Range(0f, 1f) <= switchProbability)
        {
            patrolForward = !patrolForward;

        }

        if(patrolForward)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        }
        else
        {
            if(--currentPatrolIndex < 0)
            {
                currentPatrolIndex = patrolPoints.Length - 1;
            }
        }
    }

    private void Danger()
    {
        hostile = GameObject.FindGameObjectWithTag("Hostile");

        if(Vector3.Distance(this.transform.position, hostile.transform.position) <= dangerRange)
        {
            fear++;
            safe = -1;

            adrenaline += 3;
            //Debug.Log("in danger" + this.name);
        }
        if (Vector3.Distance(this.transform.position, hostile.transform.position) <= dangerRange/2)
        {
            fear = fear * 2;
            safe = -5;
            happiness = happiness / 2;
            adrenaline += 13;
        }
        if (Vector3.Distance(this.transform.position, hostile.transform.position) <= damageRange)
        {
            HostileAgent hostileAgent = hostile.GetComponent<HostileAgent>();
            int hpLost = hostileAgent.damage;

            health = health - hpLost;
            damaged = true;
            helplesness += helplesness + 10; 
            adrenaline += 33;
            safe = -40;
            Debug.Log("damage taken" + this.name);


        }
        if (Vector3.Distance(this.transform.position, hostile.transform.position) <= damageRange/2)
        {
            trapped = true;
        }

        if (escaped == true)
        {
            HostileAgent hostileAgent = hostile.GetComponent<HostileAgent>();
            hostileAgent.remainingAgents--;
            escaped = true;
            this.gameObject.SetActive(false);
        }


    }

    private void AdrenalineIncreased()
    {
        bool msgsent;
        if(adrenaline>33)
        {
            if (this.navMeshAgent.speed > speed)
            {
                //light adrenaline so faster
                this.navMeshAgent.speed += this.navMeshAgent.speed / 2;
                Debug.Log("going faster" + this.name);
            }

        }
        if (adrenaline > 66)
        {

        }
        if (adrenaline > 75)
        {
            //too high takes damage
            //health--;
            msgsent = true;
            if (msgsent == false)
            {
                Debug.Log("adrenaline damage taken" + this.name);
            }

            //rend.material.color = Color.blue;
        }
    }

    private void CheckHp()
    {
        //just does a condition if dead
        if(health <= 0)
        {
            HostileAgent hostileAgent = hostile.GetComponent<HostileAgent>();
            hostileAgent.agentsDestroyed++;
            hostileAgent.remainingAgents--;
            this.gameObject.SetActive(false);
        }
    }
    private void FeelingSafe()
    {
        //if not feeling safe will try to hide
        if (hiding == false)
        {
            if (safe <= 50)
            {
                ChangePatrolPoint();
                hiding = true;


            }
        }
       
    }
}
