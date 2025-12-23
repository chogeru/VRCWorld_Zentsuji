using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class IntroSequencer : UdonSharpBehaviour
{
    [Header("UI References")]
    public CanvasGroup backgroundGroup;
    public CanvasGroup logoContentGroup;
    public RectTransform logoTransform;

    [Header("Animation Settings")]
    public float fadeInDuration = 2.0f;
    public float displayDuration = 3.0f;
    public float fadeOutDuration = 2.0f;
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool enableZoomEffect = true;
    public float startScale = 0.95f;
    public float endScale = 1.05f;

    [Header("Callback Settings")]
    [Tooltip("終了時にイベントを送る相手")]
    public UdonSharpBehaviour callbackTarget;
    [Tooltip("終了時に呼ぶメソッド名")]
    public string callbackMethodName = "OnIntroFinished";

    private int _currentState = 0; // 0:Idle, 1:FadeIn, 2:Display, 3:FadeOut
    private float _timer = 0f;
    private bool _isPlaying = false;

    // 自動では開始せず、Managerから呼ばれるのを待つ
    public void PlaySequence()
    {
        InitializeSystem();
        _isPlaying = true;
        _currentState = 1; // Start FadeIn
        this.gameObject.SetActive(true); // 念のためActive化
    }

    void Update()
    {
        if (!_isPlaying) return;

        _timer += Time.deltaTime;

        if (_currentState == 1) ProcessFadeIn();
        else if (_currentState == 2) ProcessDisplay();
        else if (_currentState == 3) ProcessFadeOut();
    }

    private void InitializeSystem()
    {
        _timer = 0f;
        SetGroupAlpha(backgroundGroup, 1f);
        SetGroupAlpha(logoContentGroup, 0f);
        if (logoTransform != null && enableZoomEffect)
            logoTransform.localScale = Vector3.one * startScale;
    }

    private void ProcessFadeIn()
    {
        float progress = Mathf.Clamp01(_timer / fadeInDuration);
        SetGroupAlpha(logoContentGroup, alphaCurve.Evaluate(progress));
        ApplyZoom(progress, startScale, Mathf.Lerp(startScale, endScale, 0.4f));

        if (_timer >= fadeInDuration) ChangeState(2);
    }

    private void ProcessDisplay()
    {
        // 待機中もズーム継続
        if (enableZoomEffect && logoTransform != null)
        {
            float currentScale = Mathf.Lerp(startScale, endScale, 0.4f + (_timer / displayDuration) * 0.6f);
            logoTransform.localScale = Vector3.one * currentScale;
        }

        if (_timer >= displayDuration) ChangeState(3);
    }

    private void ProcessFadeOut()
    {
        float progress = Mathf.Clamp01(_timer / fadeOutDuration);
        float alpha = 1f - Mathf.SmoothStep(0f, 1f, progress);
        SetGroupAlpha(backgroundGroup, alpha);
        SetGroupAlpha(logoContentGroup, alpha);

        if (_timer >= fadeOutDuration)
        {
            FinishSequence();
        }
    }

    private void FinishSequence()
    {
        _isPlaying = false;
        this.gameObject.SetActive(false); // UIを消す

        // Managerに「終わったよ」と報告する
        if (callbackTarget != null)
        {
            callbackTarget.SendCustomEvent(callbackMethodName);
        }
    }

    private void ChangeState(int newState)
    {
        _currentState = newState;
        _timer = 0f;
    }

    private void SetGroupAlpha(CanvasGroup group, float alpha)
    {
        if (group != null) group.alpha = alpha;
    }

    private void ApplyZoom(float t, float s, float e)
    {
        if (logoTransform != null && enableZoomEffect)
            logoTransform.localScale = Vector3.one * Mathf.Lerp(s, e, t);
    }
}