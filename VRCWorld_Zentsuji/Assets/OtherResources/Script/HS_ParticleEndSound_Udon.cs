using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HS_ParticleEndSound_Udon : UdonSharpBehaviour
{
    [Header("Explosion用設定")]
    public float explosionMinVolume = 0.3f;
    public float explosionMaxVolume = 0.7f;
    public float explosionPitchMin = 0.75f;
    public float explosionPitchMax = 1.25f;

    [Header("Shot用設定")]
    public float shootMinVolume = 0.05f;
    public float shootMaxVolume = 0.1f;
    public float shootPitchMin = 0.75f;
    public float shootPitchMax = 1.25f;

    [Header("音源クリップ")]
    public AudioClip[] audioExplosion;
    public AudioClip[] audioShot;

    [Header("参照設定")]
    // 対象の ParticleSystem（パーティクル再生オブジェクト）をアサイン
    public ParticleSystem particleSystem;

    // 各サウンド用のオブジェクトプール（事前にシーン内に配置してインスペクターから参照）
    public AudioPoolObject[] explosionPool;
    public AudioPoolObject[] shotPool;

    // プールから次に取り出すインデックス（ラウンドロビン方式）
    private int explosionPoolIndex = 0;
    private int shotPoolIndex = 0;

    // パーティクル情報を受け取るためのバッファ
    private ParticleSystem.Particle[] particlesBuffer;

    // 1フレームあたりの最大再生件数（状況に合わせて調整してください）
    [Header("同時再生数の制限")]
    public int maxExplosionSoundsPerFrame = 3;
    public int maxShotSoundsPerFrame = 3;

    void Start()
    {
        // パーティクルシステムの最大発生数に合わせてバッファを確保
        if (particleSystem != null)
        {
            particlesBuffer = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        }
    }

    void Update()
    {
        if (particleSystem == null || particlesBuffer == null) return;

        int count = particleSystem.GetParticles(particlesBuffer);
        int explosionSoundsTriggered = 0;
        int shotSoundsTriggered = 0;

        for (int i = 0; i < count; i++)
        {
            // Explosion用：パーティクルの残り寿命が deltaTime 未満ならサウンド再生
            if (audioExplosion != null && audioExplosion.Length > 0 &&
                particlesBuffer[i].remainingLifetime < Time.deltaTime)
            {
                // 同時再生数の上限に達していなければ再生する
                if (explosionSoundsTriggered < maxExplosionSoundsPerFrame)
                {
                    AudioPoolObject soundInstance = explosionPool[explosionPoolIndex];
                    explosionPoolIndex = (explosionPoolIndex + 1) % explosionPool.Length;

                    soundInstance.transform.position = particlesBuffer[i].position;
                    AudioClip clip = audioExplosion[Random.Range(0, audioExplosion.Length)];
                    float volume = Random.Range(explosionMinVolume, explosionMaxVolume);
                    float pitch = Random.Range(explosionPitchMin, explosionPitchMax);
                    soundInstance.PlaySound(clip, volume, pitch);

                    explosionSoundsTriggered++;
                }
            }

            // Shot用：パーティクルの生成直後（残り寿命が開始寿命に近い場合）にサウンド再生
            if (audioShot != null && audioShot.Length > 0 &&
                particlesBuffer[i].remainingLifetime >= particlesBuffer[i].startLifetime - Time.deltaTime)
            {
                if (shotSoundsTriggered < maxShotSoundsPerFrame)
                {
                    AudioPoolObject soundInstance = shotPool[shotPoolIndex];
                    shotPoolIndex = (shotPoolIndex + 1) % shotPool.Length;

                    soundInstance.transform.position = particlesBuffer[i].position;
                    AudioClip clip = audioShot[Random.Range(0, audioShot.Length)];
                    float volume = Random.Range(shootMinVolume, shootMaxVolume);
                    float pitch = Random.Range(shootPitchMin, shootPitchMax);
                    soundInstance.PlaySound(clip, volume, pitch);

                    shotSoundsTriggered++;
                }
            }
        }
    }
}
