using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;

[Title("Open URL Toggle")]
public class OpenURL : UdonSharpBehaviour
{
    [FoldoutGroup("対象オブジェクト"), Required, Tooltip("インタラクト時に表示/非表示切り替える InputField のオブジェクト")]
    public GameObject inputFieldObject;

    [FoldoutGroup("対象オブジェクト"), Required, Tooltip("インタラクト時に表示/非表示切り替える Image のオブジェクト")]
    public GameObject imageObject;

    [FoldoutGroup("デバッグ"), Button("Toggle Objects")]
    private void ToggleObjects()
    {
        Interact();
    }

    public override void Interact()
    {
        if (inputFieldObject == null || imageObject == null)
        {
            Debug.LogWarning("対象オブジェクトが設定されていません", this);
            return;
        }

        bool newState = !inputFieldObject.activeSelf;
        inputFieldObject.SetActive(newState);
        imageObject.SetActive(newState);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        if (inputFieldObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, inputFieldObject.transform.position);
            Gizmos.DrawWireCube(inputFieldObject.transform.position, Vector3.one);
        }

        if (imageObject != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, imageObject.transform.position);
            Gizmos.DrawWireCube(imageObject.transform.position, Vector3.one);
        }
    }
}
