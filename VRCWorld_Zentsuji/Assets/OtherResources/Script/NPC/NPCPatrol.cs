using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

public enum PatrolState
{
    Moving,
    Rotating,
    Waiting
}
public class NPCPatrol : UdonSharpBehaviour
{

    [Header("巡回地点設定")]
    [Tooltip("巡回地点の配列をここで設定してください。順番は巡回する順序になります。")]
    public PatrolPoint[] patrolPoints;

    [Header("回転速度")]
    [Tooltip("待機時の回転速度（待機地点で指定された回転角度にスムーズに合わせます）。")]
    public float rotationSpeed = 5f;

    [Header("Animator設定")]
    public string walkParameterName = "Walk";

    public NavMeshAgent agent;
    public Animator animator;
    private int currentIndex = 0;
    private PatrolState currentState = PatrolState.Moving; // 外部定義した PatrolState を使用

    // 回転・待機用の内部変数
    private Quaternion targetRotation;
    private float waitTimer = 0f;
    private const float rotationThreshold = 1f; // 回転終了判定用の角度誤差（度）

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("巡回地点が設定されていません : " + gameObject.name);
            return;
        }

        // 最初の巡回地点を設定
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
                        // 待機地点の場合、指定された回転に向けて回転開始
                        targetRotation = Quaternion.Euler(currentPoint.waitRotation);
                        currentState = PatrolState.Rotating;
                    }
                    else
                    {
                        // 待機せず、即次の地点へ進む
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

    private void ProceedToNextWaypoint()
    {
        currentIndex = (currentIndex + 1) % patrolPoints.Length;
        SetNextDestination();
    }

    private void SetNextDestination()
    {
        currentState = PatrolState.Moving;
        SetWalkAnimation();
        agent.destination = patrolPoints[currentIndex].transform.position;
    }

    private void SetWalkAnimation()
    {
        if (animator != null)
            animator.SetBool(walkParameterName, true);
    }

    private void SetIdleAnimation()
    {
        if (animator != null)
            animator.SetBool(walkParameterName, false);
    }
}
