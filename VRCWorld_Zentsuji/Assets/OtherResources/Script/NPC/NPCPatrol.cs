using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 巡回状態の列挙体
/// </summary>
public enum PatrolState
{
    Moving,
    Rotating,
    Waiting
}

/// <summary>
/// NPCの巡回処理を行うクラス
/// </summary>
public class NPCPatrol : UdonSharpBehaviour
{
    [Header("巡回地点設定")]
    [Tooltip("巡回地点の配列をここで設定してください。順番は巡回する順序になります")]
    public PatrolPoint[] patrolPoints;

    [Header("回転速度")]
    public float rotationSpeed = 5f;

    [Header("Animator設定")]
    public string walkParameterName = "Walk";

    public NavMeshAgent agent;
    public Animator animator;
    private int currentIndex = 0;
    private PatrolState currentState = PatrolState.Moving;

    private Quaternion targetRotation;
    private float waitTimer = 0f;
    private const float rotationThreshold = 1f;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        SetNextDestination();
    }


    void Update()
    {
        if (agent == null || patrolPoints == null || patrolPoints.Length == 0) return;

        switch (currentState)
        {
            case PatrolState.Moving:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    SetIdleAnimation();

                    PatrolPoint currentPoint = patrolPoints[currentIndex];
                    if (!currentPoint.isDoNotStop)
                    {
                        targetRotation = Quaternion.Euler(currentPoint.waitRotation);
                        currentState = PatrolState.Rotating;
                    }
                    else
                    {
                        ProceedToNextWaypoint();
                    }
                }
                break;

            case PatrolState.Rotating:
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                if (Quaternion.Angle(transform.rotation, targetRotation) <= rotationThreshold)
                {
                    transform.rotation = targetRotation;
                    waitTimer = 0f;
                    currentState = PatrolState.Waiting;
                }
                break;

            case PatrolState.Waiting:
                waitTimer += Time.deltaTime;
                PatrolPoint pt = patrolPoints[currentIndex];
                if (waitTimer >= pt.waitTime)
                {
                    ProceedToNextWaypoint();
                }
                break;
        }
    }

    /// <summary>
    /// 次の巡回地点へ移行する処理
    /// 現在のインデックスを更新し、次の目的地を設定します。
    /// </summary>
    private void ProceedToNextWaypoint()
    {
        currentIndex = (currentIndex + 1) % patrolPoints.Length;
        SetNextDestination();
    }

    /// <summary>
    /// 次の巡回地点を設定する処理
    /// 巡回状態を移動状態に切り替え、歩行アニメーションを再生し、NavMeshAgentの目的地を更新
    /// </summary>
    private void SetNextDestination()
    {
        currentState = PatrolState.Moving;
        SetWalkAnimation();
        agent.destination = patrolPoints[currentIndex].transform.position;
    }

    /// <summary>
    /// 歩行中のアニメーションを設定する処理
    /// Animatorのパラメータをtrueに設定
    /// </summary>
    private void SetWalkAnimation()
    {
        if (animator != null)
            animator.SetBool(walkParameterName, true);
    }

    /// <summary>
    /// 待機中のアニメーションを設定する処理
    /// Animatorのパラメータをfalseに設定
    /// </summary>
    private void SetIdleAnimation()
    {
        if (animator != null)
            animator.SetBool(walkParameterName, false);
    }
}
