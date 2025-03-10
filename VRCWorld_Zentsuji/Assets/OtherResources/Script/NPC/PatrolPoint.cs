using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PatrolPoint : UdonSharpBehaviour
{
    [Header("待機時間")]
    [Tooltip("この地点での待機時間")]
    public float waitTime = 2f;

    [Header("待機時回転方向 (Euler角)")]
    [Tooltip("待機中に向く方向")]
    public Vector3 waitRotation;

    [Header("待機しない場合はチェック")]
    [Tooltip("チェックすると、この地点到着後は待機・回転を行わず、即次の地点へ移動を行う")]
    public bool isDoNotStop = false;
}
