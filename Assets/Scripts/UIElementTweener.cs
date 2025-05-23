using System;
using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
public class UIElementTweener : MonoBehaviour
{
    [SerializeField]
    [Header("Animation Settings")]
    [Tooltip("Start scale when showing")]
    private Vector3 startScale = Vector3.one * 0f;

    [SerializeField]
    [Tooltip("Overshoot scale")]
    private Vector3 overshootScale = Vector3.one * 1.25f;

    [SerializeField]
    [Tooltip("End scale after showing")] 
    private Vector3 endScale = Vector3.one;

    [SerializeField]
    [Tooltip("Duration of show animation in seconds")]
    private float showDuration = 0.3f;

    [SerializeField]
    [Tooltip("Duration of hide animation in seconds")]
    private float hideDuration = 0.2f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Ease type for show animation")]
    protected float overshootRatio = 0.6f;

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Portion of showDuration for settling to final scale (0 to 1)")]
    protected float settleRatio = 0.4f;

    [SerializeField]
    [Tooltip("Ease type for show animation")]
    private Ease showEase = Ease.OutBack;

    [SerializeField]
    [Tooltip("Ease type for hide animation")]
    private Ease hideEase = Ease.InBack;

    public event Action OnShowPeak;
    public event Action OnShowComplete;
    public event Action OnHideComplete;

    private Sequence _showSequence;

    private Tweener _overshootTweener;
    private Tweener _settleTweener;
    private Tweener _hideTweener;


    public bool IsVisible { get; private set; }

    public void Show()
    {
        Debug.Log($"Show {gameObject.name}");
        KillTweens();
        transform.localScale = startScale;
        IsVisible = true;

        _overshootTweener = transform
            .DOScale(overshootScale, showDuration * overshootRatio)
            .SetEase(showEase)
            .OnComplete(() => OnShowPeak?.Invoke());

        _settleTweener = transform
            .DOScale(endScale, showDuration * settleRatio)
            .SetEase(showEase)
            .OnComplete(() =>
                OnShowComplete?.Invoke());

        _showSequence = DOTween.Sequence()
            .Append(_overshootTweener)
            .Append(_settleTweener);
    }

    public void Hide()
    {
        KillTweens();
        IsVisible = false;

        _hideTweener = transform
            .DOScale(Vector3.zero, hideDuration)
            .SetEase(hideEase)
            .OnComplete(() => OnHideComplete?.Invoke());
    }

    public void Toggle()
    {
        if (IsVisible)
        {
            Hide();
        }
        else Show();
    }

    private void KillTweens()
    {
        if (_showSequence?.IsActive() == true) _showSequence.Kill();
        if (_hideTweener?.IsActive() == true) _hideTweener.Kill();

        _showSequence = null;
        _hideTweener = null;
    }

    protected virtual void OnDestroy()
    {
        KillTweens();
        OnShowPeak = OnShowComplete = OnHideComplete = null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIElementTweener))]
public class UIElementTweenerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        UIElementTweener tweener = (UIElementTweener)target;
        EditorGUILayout.Space();

        // Show toggle button only in play mode for runtime debugging
        if (Application.isPlaying)
        {
            if (GUILayout.Button(tweener.IsVisible ? "Hide Element" : "Show Element"))
            {
                tweener.Toggle();
            }
        }
    }
}
#endif