using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;
using UnityEngine.Events;

[Title("Interactive Active Toggle")]
public class InteractiveActiveToggle : UdonSharpBehaviour
{
    [FoldoutGroup("対象オブジェクト"), Required, Tooltip("トグルでアクティブ/非アクティブを切り替える対象オブジェクト")]
    public GameObject targetObject;

    [FoldoutGroup("設定"), Tooltip("回転させる角度（通常は180度）")]
    public float rotationAngle = 180f;

    [FoldoutGroup("設定"), Tooltip("回転処理を行うかどうかのトリガー")]
    public bool enableRotation = true;

    [FoldoutGroup("設定"), Tooltip("回転軸を指定します (例: (0,1,0) はY軸)")]
    public Vector3 rotationAxis = Vector3.up;

    public override void Interact()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("targetObjectが設定されていない", this);
            return;
        }

        bool newState = ToggleActiveState();

        if (enableRotation)
        {
            UpdateRotation(newState);
        }
    }

    protected virtual bool ToggleActiveState()
    {
        bool newState = !targetObject.activeSelf;
        targetObject.SetActive(newState);
        return newState;
    }

    protected virtual void UpdateRotation(bool state)
    {
        if (state)
        {
            transform.localRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (targetObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);
            Gizmos.DrawWireCube(targetObject.transform.position, Vector3.one);
        }
    }
}
