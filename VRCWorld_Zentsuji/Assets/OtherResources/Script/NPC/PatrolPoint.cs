using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;

[Title("巡回地点設定")]
public class PatrolPoint : UdonSharpBehaviour
{
    [FoldoutGroup("待機設定"), LabelText("待機時間 (秒)"), Tooltip("この地点での待機時間")]
    public float waitTime = 2f;

    [FoldoutGroup("待機設定"), LabelText("待機時回転方向 (Euler角)"), Tooltip("待機中に向く方向")]
    public Vector3 waitRotation;

    [FoldoutGroup("オプション"), LabelText("待機しない"), Tooltip("チェックすると、この地点到着後は待機・回転を行わず、即次の地点へ移動を行う")]
    public bool isDoNotStop = false;
}
