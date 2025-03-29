using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;

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
/// NPCの巡回処理を行うクラス（改善版）
/// </summary>
[Title("NPC 巡回設定")]
public class NPCPatrol : UdonSharpBehaviour
{
    [FoldoutGroup("巡回設定"), Tooltip("巡回地点の配列をここで設定。順番は巡回する順序")]
    public PatrolPoint[] patrolPoints;

    [FoldoutGroup("巡回設定"), LabelText("回転速度"), SuffixLabel("deg/sec")]
    public float rotationSpeed = 5f;

    [FoldoutGroup("Animator設定"), LabelText("歩行アニメーションパラメータ名")]
    public string walkParameterName = "Walk";

    [FoldoutGroup("コンポーネント"), Required, LabelText("NavMesh Agent")]
    public NavMeshAgent agent;

    [FoldoutGroup("コンポーネント"), Required, LabelText("Animator")]
    public Animator animator;

    private int currentIndex = 0;
    private PatrolState currentState = PatrolState.Moving;
    private Quaternion targetRotation;
    private float waitTimer = 0f;
    private const float rotationThreshold = 1f;

    void Start()
    {
        if (agent == null) { agent = GetComponent<NavMeshAgent>(); }
        if (animator == null) { animator = GetComponent<Animator>(); }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("Patrol points are not set.", this);
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
                HandleMovingState();
                break;
            case PatrolState.Rotating:
                HandleRotatingState();
                break;
            case PatrolState.Waiting:
                HandleWaitingState();
                break;
        }
    }

    /// <summary>
    /// 移動状態の処理
    /// </summary>
    private void HandleMovingState()
    {
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
    }

    /// <summary>
    /// 回転状態の処理
    /// </summary>
    private void HandleRotatingState()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        if (Quaternion.Angle(transform.rotation, targetRotation) <= rotationThreshold)
        {
            transform.rotation = targetRotation;
            waitTimer = 0f;
            currentState = PatrolState.Waiting;
        }
    }

    /// <summary>
    /// 待機状態の処理
    /// </summary>
    private void HandleWaitingState()
    {
        waitTimer += Time.deltaTime;
        PatrolPoint pt = patrolPoints[currentIndex];
        if (waitTimer >= pt.waitTime)
        {
            ProceedToNextWaypoint();
        }
    }

    /// <summary>
    /// 次の巡回地点へ移行する処理
    /// </summary>
    private void ProceedToNextWaypoint()
    {
        currentIndex = (currentIndex + 1) % patrolPoints.Length;
        SetNextDestination();
    }

    /// <summary>
    /// 次の巡回地点を設定する処理
    /// </summary>
    private void SetNextDestination()
    {
        currentState = PatrolState.Moving;
        SetWalkAnimation();
        agent.destination = patrolPoints[currentIndex].transform.position;
    }

    /// <summary>
    /// 歩行アニメーションを開始する処理
    /// </summary>
    private void SetWalkAnimation()
    {
        if (animator != null)
            animator.SetBool(walkParameterName, true);
    }

    /// <summary>
    /// 待機アニメーションを設定する処理
    /// </summary>
    private void SetIdleAnimation()
    {
        if (animator != null)
            animator.SetBool(walkParameterName, false);
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタ上で巡回経路を視覚化するためのGizmos描画
    /// </summary>
    void OnDrawGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].transform.position, 0.2f);
                int nextIndex = (i + 1) % patrolPoints.Length;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].transform.position, patrolPoints[nextIndex].transform.position);
                }
            }
        }
    }
#endif
}
