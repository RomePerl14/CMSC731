// ||                            ||
// ||       Romeo Perlstein      ||
// ||  CMSC731 - Advances in XR  ||
// ||  HW1: Roll-a-ball Tutorial ||
// ||                            ||

using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{

    public Transform player;
    
    // private variables
    private NavMeshAgent navMeshAgent;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(player != null)
        {
            navMeshAgent.SetDestination(player.position);
        }
    }
}
