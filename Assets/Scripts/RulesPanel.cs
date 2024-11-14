using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulesPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private TMP_Text buttonArrow;

    private bool isOpened = false;

    private void Start()
    {
        SetOpen(true);
    }

    public void Toggle()
    {
        SetOpen(!isOpened);
    }

    public void SetOpen(bool open)
    {
        if (isOpened == open)
            return;

        isOpened = open;

        float targetX = isOpened ? 0 : -panelTransform.rect.width;
        panelTransform.DOAnchorPosX(targetX, 0.5f);
        buttonArrow.text = isOpened ? "<" : ">";
    }
}
