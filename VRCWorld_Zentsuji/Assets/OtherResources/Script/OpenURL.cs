using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class OpenURL : UdonSharpBehaviour
{
    [Header("インタラクト時に表示/非表示切り替えるobject")]
    public GameObject inputFieldObject;
    public GameObject imageObject;

    public override void Interact()
    {
        if (inputFieldObject == null || imageObject == null)
        {
            return;
        }

        bool currentState = inputFieldObject.activeSelf;
        bool newState = !currentState;

        inputFieldObject.SetActive(newState);
        imageObject.SetActive(newState);
    }
}
