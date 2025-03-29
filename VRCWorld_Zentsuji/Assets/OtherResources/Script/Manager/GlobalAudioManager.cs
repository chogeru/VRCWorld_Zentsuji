using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Sirenix.OdinInspector;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
[Title("Global Audio Manager")]
public class GlobalAudioManager : UdonSharpBehaviour
{
    [FoldoutGroup("Audio Settings"), Required, Tooltip("全体で音声再生に利用する AudioSource を設定してください")]
    public AudioSource audioSource;

    [FoldoutGroup("Debug"), Button("テスト再生")]
    private void DebugTestPlay()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource が設定されていない", this);
        }
    }

    /// <summary>
    /// 指定した AudioClip を一度だけ再生
    /// </summary>
    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource が設定されてない", this);
            return;
        }
        if (clip == null)
        {
            Debug.LogWarning("再生するAudioClipがnull", this);
            return;
        }
        audioSource.PlayOneShot(clip);
    }
}
