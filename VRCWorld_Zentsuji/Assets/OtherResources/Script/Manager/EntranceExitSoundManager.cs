using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;

[Title("入出音再生システム")]
public class EntranceExitSoundManager : UdonSharpBehaviour
{
    [FoldoutGroup("オーディオ設定"), Required, Tooltip("ユーザー入室時に再生する音声クリップ")]
    public AudioClip enterSound;

    [FoldoutGroup("オーディオ設定"), Required, Tooltip("ユーザー退室時に再生する音声クリップ")]
    public AudioClip exitSound;

    [FoldoutGroup("オーディオ設定"), Required, Tooltip("GlobalAudioManager コンポーネントを持つオブジェクトを設定してください")]
    public GlobalAudioManager globalAudioManager;

    [FoldoutGroup("デバッグ"), Button("入室サウンドテスト")]
    private void TestEnterSound()
    {
        if (enterSound != null && globalAudioManager != null)
        {
            globalAudioManager.PlayOneShot(enterSound);
        }
        else
        {
            Debug.LogWarning("入室サウンドまたは GlobalAudioManager が設定されていない", this);
        }
    }

    [FoldoutGroup("デバッグ"), Button("退室サウンドテスト")]
    private void TestExitSound()
    {
        if (exitSound != null && globalAudioManager != null)
        {
            globalAudioManager.PlayOneShot(exitSound);
        }
        else
        {
            Debug.LogWarning("退室サウンドまたは GlobalAudioManager が設定されていない", this);
        }
    }

    /// <summary>
    /// ユーザーが入室した際に各クライアントで呼ばれる
    /// </summary>
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (enterSound != null && globalAudioManager != null)
        {
            globalAudioManager.PlayOneShot(enterSound);
        }
    }

    /// <summary>
    /// ユーザーが退室した際に各クライアントで呼ばれる
    /// </summary>
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (exitSound != null && globalAudioManager != null)
        {
            globalAudioManager.PlayOneShot(exitSound);
        }
    }
}
