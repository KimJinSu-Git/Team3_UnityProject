using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MoveCharacterController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;
    
    private Transform[] targetPosition;
    private bool walk;
    private int brokenCastle = 1;
    
    private void Start()
    {
        walk = false;
        TryGetComponent(out agent);
        TryGetComponent(out animator);
        
        // 부모에서 DestinationSetting 컴포넌트를 찾아 targetPosition 가져오기
        DestinationSetting destinationSetting = GetComponentInParent<DestinationSetting>();
        targetPosition = destinationSetting.targetPosition;
        
        BrokenCastleManager.OnBrokenCastle += SetDestination;
        
        //초기 위치 설정
        SetDestination(brokenCastle);
    }

    private void Update()
    {
        switch (brokenCastle)
        {
            case 0:
                MoveToClosest(0, 1, 2);
                break;
            case 1:
                MoveToClosest(1, 2);
                break;
            case 2:
                MoveToClosest(0, 2);
                break;
            case 3:
                agent.isStopped = true; // 이동 중단
                break;
        }
    }

    private void MoveToClosest(int indexA, int indexB, int indexC = -1)
    {
        float distA = Vector3.Distance(transform.position, targetPosition[indexA].position);
        float distB = Vector3.Distance(transform.position, targetPosition[indexB].position);
        float distC = indexC >= 0 ? Vector3.Distance(transform.position, targetPosition[indexC].position) : float.MaxValue;

        int closestIndex = indexA;
        float minDist = distA;

        if (distB < minDist)
        {
            closestIndex = indexB;
            minDist = distB;
        }

        if (indexC >= 0 && distC < minDist)
        {
            closestIndex = indexC;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition[closestIndex].position);
        }
    }

    private void SetDestination(int castleState)
    {
        brokenCastle = castleState;

        if (brokenCastle == 3)
        {
            agent.isStopped = true;
        }
    }

    private void OnDestroy()
    {
        BrokenCastleManager.OnBrokenCastle -= SetDestination;
    }
}
