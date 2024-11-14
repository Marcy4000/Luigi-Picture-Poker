using DG.Tweening;
using JSAM;
using UnityEngine;

public enum CardType { Star, Mario, Luigi, Flower, Mushroom, Cloud }

public class Card : MonoBehaviour
{
    [SerializeField] private GameObject cardPositionHolder, cardRotationHolder;
    [SerializeField] private SpriteRenderer frontSpriteRenderer, backSpriteRenderer;

    [SerializeField] private CardType cardType;
    [SerializeField] private bool isFlipped = false;
    [SerializeField] private bool isSelected = false;
    [SerializeField] private bool canCardBeSelected = false;

    public CardType CardType => cardType;
    public bool IsFlipped { get => isFlipped; }
    public bool IsSelected { get => isSelected; set => SetCardSelected(value); }
    public bool CanCardBeSelected { get => canCardBeSelected; set => canCardBeSelected = value; }

    public void InitializeCard(CardType type)
    {
        cardType = type;
        UpdateCardSprite();
        DeckSkinManager.instance.OnDeckSkinChanged += UpdateCardSprite;
        FlipCard(false, false);
    }

    private void OnDestroy()
    {
        DeckSkinManager.instance.OnDeckSkinChanged -= UpdateCardSprite;
    }

    private void UpdateCardSprite()
    {
        if (frontSpriteRenderer == null || backSpriteRenderer == null)
        {
            return;
        }

        frontSpriteRenderer.sprite = DeckSkinManager.instance.CurrentDeckSkin.GetSpritesForType(cardType).cardSprite;
        backSpriteRenderer.sprite = DeckSkinManager.instance.CurrentDeckSkin.cardBack;
    }

    public void FlipCard()
    {
        isFlipped = !isFlipped;

        DoFlipAnimation();
    }

    private void SetCardSelected(bool value)
    {
        isSelected = value;

        float targetY = value ? 0.15f : 0f;
        cardPositionHolder.transform.DOLocalMoveY(targetY, 0.2f).SetEase(Ease.InOutSine);
    }

    public void FlipCard(bool value, bool doAnimation)
    {
        isFlipped = value;

        if (doAnimation)
            DoFlipAnimation();
        else
            cardRotationHolder.transform.localRotation = Quaternion.Euler(0, isFlipped ? 180 : 0, 0);
    }

    private void DoFlipAnimation()
    {
        float targetRotation = isFlipped ? 180f : 0f;

        cardRotationHolder.transform.DOKill();

        cardRotationHolder.transform.DOLocalMoveZ(0.1f, 0.4f).OnComplete(() =>
        {
            AudioManager.PlaySound(DefaultSounds.FlipCard);
            cardRotationHolder.transform.DOLocalRotate(new Vector3(0, targetRotation, 0), 0.35f, RotateMode.FastBeyond360).onComplete += () =>
            {
                cardRotationHolder.transform.DOLocalMoveZ(0, 0.4f);
            };
        });
    }

    private void OnMouseDown()
    {
        if (canCardBeSelected)
        {
            IsSelected = !IsSelected;

            AudioManager.PlaySound(isSelected ? DefaultSounds.CardSelected : DefaultSounds.CardDeselected); 
        }
    }

    private void OnMouseEnter()
    {
        if (!canCardBeSelected)
            return;

        cardRotationHolder.transform.DOKill();
        cardRotationHolder.transform.DOLocalMoveZ(0.015f, 0.2f).SetEase(Ease.InOutSine);
        cardRotationHolder.transform.DOLocalRotate(new Vector3(5, isFlipped ? 180 : 0, 5), 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnMouseExit()
    {
        if (!canCardBeSelected)
            return;

        cardRotationHolder.transform.DOKill();
        cardRotationHolder.transform.DOLocalMoveZ(0, 0.2f).SetEase(Ease.InOutSine);
        cardRotationHolder.transform.DOLocalRotate(new Vector3(0, isFlipped ? 180 : 0, 0), 0.2f).SetEase(Ease.InOutSine);
    }
}
