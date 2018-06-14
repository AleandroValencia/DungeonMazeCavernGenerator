using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathFinderScript : MonoBehaviour
{
    public Transform target;
    NavMeshAgent m_navMesAgent;
    // Use this for initialization
    void Start ()
    {
        m_navMesAgent = this.GetComponent<NavMeshAgent>();
        m_navMesAgent.SetDestination(target.position);
	}

    void Update()
    {
        if (!m_navMesAgent.pathPending)
        {
            if (m_navMesAgent.remainingDistance <= m_navMesAgent.stoppingDistance)
            {
                if (!m_navMesAgent.hasPath || m_navMesAgent.velocity.sqrMagnitude == 0f)
                {
                    //Debug.Log("found destination");
                    Destroy(this.gameObject, 200);
                }
            }
        }
    }
}
