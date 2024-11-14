using DG.Tweening;
using UnityEngine;

public class SkinSelector : MonoBehaviour
{
    [SerializeField] private SkinSelectorItemUI skinSelectorItemPrefab;
    [SerializeField] private Transform skinSelectorItemHolder;
    [SerializeField] private RectTransform skinSelectorHolder;

    [SerializeField] private GameObject blockCardSelection;

    private bool isMenuOpen = false;

    private void Start()
    {
        foreach (DeckSkin skin in DeckSkinManager.instance.DeckSkins)
        {
            SkinSelectorItemUI skinSelectorItem = Instantiate(skinSelectorItemPrefab, skinSelectorItemHolder);
            skinSelectorItem.Initialize(skin);
            skinSelectorItem.OnSelectButtonClicked += OnSkinSelected;
        }

        CloseSkinSelector();
    }

    private void OnSkinSelected(DeckSkin skin)
    {
        DeckSkinManager.instance.ChangeDeckSkin(skin);
    }

    public void ToggleSkinSelector()
    {
        isMenuOpen = !isMenuOpen;

        float targetY = isMenuOpen ? 0 : -skinSelectorHolder.rect.height - 100;
        skinSelectorHolder.DOAnchorPosY(targetY, 0.2f).SetEase(Ease.InOutSine);

        blockCardSelection.SetActive(isMenuOpen);
    }

    public void CloseSkinSelector()
    {
        isMenuOpen = false;

        skinSelectorHolder.DOAnchorPosY(-skinSelectorHolder.rect.height - 100, 0.2f).SetEase(Ease.InOutSine);

        blockCardSelection.SetActive(isMenuOpen);
    }

    public void OpenSkinSelector()
    {
        isMenuOpen = true;

        skinSelectorHolder.DOAnchorPosY(0, 0.2f).SetEase(Ease.InOutSine);

        blockCardSelection.SetActive(isMenuOpen);
    }
}
