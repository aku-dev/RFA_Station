/* =======================================================================================================
 * AK Studio
 * 
 * NPC System
 * Version 1.0 by Alexandr Kuznecov
 * 03.03.2023
 * =======================================================================================================
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.Image;

public class NPCCleanerSystem : MonoBehaviour, IDestructible
{
    [SerializeField] private NavMeshAgent[] m_NavMeshAgents = null;
    [SerializeField] private float m_Distance = 30.0f;

    private NPCCleaner[] arrayOfCleaners = null;
    private float lastCheckPlayerDistance = 0;

    private void Start()
    {
        arrayOfCleaners = new NPCCleaner[m_NavMeshAgents.Length];

        for (int i = 0; i < m_NavMeshAgents.Length; i++)
        {
            ref NavMeshAgent o = ref m_NavMeshAgents[i];
            arrayOfCleaners[i] = new NPCCleaner(o);
        }
    }

    private void FixedUpdate()
    {
        float t = Time.time;
        float sqrtDistance = m_Distance * m_Distance;
        bool isUpdateVisible = t - lastCheckPlayerDistance > 0.5f;

        foreach (NPCCleaner c in arrayOfCleaners)
        {
            if (c.isActive)
            {
                c.Update(Time.fixedDeltaTime);
                if (isUpdateVisible)
                    c.SetVisible((c.agent.transform.position - PlayerController.MainCharacterController.transform.position).sqrMagnitude < sqrtDistance);
            }
        }

        if (isUpdateVisible) lastCheckPlayerDistance = t;
    }

    public void Hit(RaycastHit hit, float damage)
    {
        int id = hit.collider.GetInstanceID();
        foreach (NPCCleaner c in arrayOfCleaners)
        {
            if (c.id == id)
            {
                c.isActive = false;
                c.agent.isStopped = true;
                hit.collider.enabled = false;
                hit.collider.transform.Find("Mesh").gameObject.SetActive(false);
                hit.collider.transform.Find("Broken").gameObject.SetActive(true);
                GameManager.GameStatistic.enemys++;                                
            }
        }
    }
}

internal class NPCCleaner
{    
    private const float RADIUS_TARGET = 6.0f;
    private const float TURN_SPEED = 10.0f;

    public int id = 0;
    public NavMeshAgent agent = null;    
    public bool isActive = true;

    private bool isVisible = true;
    private float lastChangeTarget = 0;
    private float timeChangeTarget = 5.0f;

    public NPCCleaner(NavMeshAgent a)
    {
        agent = a;
        agent.updateRotation = false;
        agent.updatePosition = true;

        agent.SetDestination(RandomNavMeshSphere());
        
        id = agent.GetComponent<Collider>().GetInstanceID();
    }

    public void Update(float d)
    {
        if (!isActive || !isVisible) return;

        lastChangeTarget += d;        

        if (lastChangeTarget >= timeChangeTarget)
        {
            agent.SetDestination(RandomNavMeshSphere());
            lastChangeTarget = 0;
        }

        if (agent.velocity != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            agent.transform.localRotation = Quaternion.Lerp(agent.transform.localRotation, targetRotation, d * TURN_SPEED);
        }
    }

    private Vector3 RandomNavMeshSphere()
    {
        timeChangeTarget = Random.Range(5.0f, 3.0f);

        Vector3 randDirection = Random.insideUnitSphere * RADIUS_TARGET;
        NavMeshHit hit;

        randDirection += agent.transform.position;
        NavMesh.SamplePosition(randDirection, out hit, RADIUS_TARGET, Physics.DefaultRaycastLayers);
   
        return hit.position;
    }

    public void SetVisible(bool v)
    {
        if (!isActive) return;
        isVisible = v;
        agent.isStopped = !v;
    }
}