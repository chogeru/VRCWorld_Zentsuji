using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioPoolObject : UdonSharpBehaviour
{
    // AudioSource はあらかじめインスペクターから設定してください
    public AudioSource audioSource;

    /// <summary>
    /// 指定した AudioClip を、指定の音量とピッチで再生し、
    /// その AudioClip の再生時間に合わせてプール返却のタイミングを設定します。
    /// </summary>
    /// <param name="clip">再生する AudioClip</param>
    /// <param name="volume">音量</param>
    /// <param name="pitch">ピッチ</param>
    public void PlaySound(AudioClip clip, float volume, float pitch)
    {
        if (clip == null)
        {
            // クリップが null なら何もしない
            return;
        }

        gameObject.SetActive(true);
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();

        // AudioClip の再生時間に合わせる。clip.length が 0 なら 1.5 秒のデフォルトを使用
        float delay = (clip.length > 0f) ? clip.length : 1.5f;
        SendCustomEventDelayedSeconds(nameof(ReturnToPool), delay);
    }

    /// <summary>
    /// プールへ返却する処理。呼ばれると自身を非アクティブにします。
    /// </summary>
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
    }
}
