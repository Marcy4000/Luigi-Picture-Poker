using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICircleFade : MonoBehaviour
{
    public static UICircleFade Instance { get; private set; }

    [SerializeField] private RectTransform circleFade;
    [SerializeField] private RectTransform circleFadeBg;
    [SerializeField] private RectTransform referenceSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        circleFadeBg.sizeDelta = new Vector2(referenceSize.sizeDelta.x, referenceSize.sizeDelta.y);
    }

    public Tween DoFadeOut(float duration)
    {
        if (circleFade == null)
        {
            Debug.LogWarning("circleFade not assigned");
            return DOTween.To(() => 0, x => { }, 0, 0);
        }

        circleFade.gameObject.SetActive(true);

        float targetW = referenceSize.sizeDelta.x + 500f;
        float targetH = referenceSize.sizeDelta.y + 500f;
        circleFade.gameObject.SetActive(true);
        Tween t = circleFade.DOSizeDelta(new Vector2(targetW, targetH), duration).SetEase(Ease.InOutQuad);
        t.onComplete += () =>
        {
            circleFade.gameObject.SetActive(false);
        };

        return t;
    }

    public Tween DoFadeIn(float duration)
    {
        if (circleFade == null)
        {
            Debug.LogWarning("circleFade not assigned");
            return DOTween.To(() => 0, x => { }, 0, 0);
        }

        circleFade.gameObject.SetActive(true);

        float targetW = 0f;
        float targetH = 0f;
        var t = circleFade.DOSizeDelta(new Vector2(targetW, targetH), duration).SetEase(Ease.InOutQuad);
        t.OnComplete(() => circleFade.gameObject.SetActive(false));
        return t;
    }
}
