using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text skinNameText, skinDescriptionText;
    [SerializeField] private Image backFace, starCard, marioCard, luigiCard, background;
    [SerializeField] private GameObject lockedIcon;
    [SerializeField] private Button selectButton;
    [SerializeField] private Sprite[] selectedSprites;

    public event System.Action<DeckSkin> OnSelectButtonClicked;

    private DeckSkin skinData;
    private bool isSkinLocked = false;

    public void Initialize(DeckSkin skin)
    {
        skinData = skin;

        skinNameText.text = skin.skinName;
        skinDescriptionText.text = skin.skinDescription;
        skinDescriptionText.text += "\n" + GetUnlockMethodString();

        backFace.sprite = skin.cardBack;
        starCard.sprite = skin.starSprites.cardSprite;
        marioCard.sprite = skin.marioSprites.cardSprite;
        luigiCard.sprite = skin.luigiSprites.cardSprite;

        isSkinLocked = !DeckSkinManager.instance.IsSkinUnlocked(skin);
        lockedIcon.SetActive(isSkinLocked);

        background.sprite = DeckSkinManager.instance.CurrentDeckSkin == skin ? selectedSprites[1] : selectedSprites[0];

        UpdateButtonText();

        selectButton.onClick.AddListener(OnSelectButtonClickedHandler);

        DeckSkinManager.instance.OnDeckSkinChanged += OnDeckSkinChangedHandler;
        DeckSkinManager.instance.OnDeckSkinUnlocked += OnDeckUnlockedHandler;
    }

    private void OnSelectButtonClickedHandler()
    {
        if (isSkinLocked)
        {
            if (skinData.unlockMethod == UnlockMethod.Purchase && GameManager.instance.Coins >= skinData.unlockCost)
            {
                DeckSkinManager.instance.UnlockSkin(skinData);
                GameManager.instance.RemoveCoins(skinData.unlockCost);
            }

            return;
        }

        OnSelectButtonClicked?.Invoke(skinData);
    }

    private string GetUnlockMethodString()
    {
        switch (skinData.unlockMethod)
        {
            case UnlockMethod.None:
                return "Unlocked by default";
            case UnlockMethod.Coins:
                return $"Reach {skinData.unlockCost} coins";
            case UnlockMethod.Stars:
                return $"Reach {skinData.unlockCost} stars";
            case UnlockMethod.Purchase:
                return $"Buy for {skinData.unlockCost} coins";
            default:
                return "";
        }
    }

    private void OnDeckSkinChangedHandler()
    {
        background.sprite = DeckSkinManager.instance.CurrentDeckSkin == skinData ? selectedSprites[1] : selectedSprites[0];
        UpdateButtonText();
    }

    private void OnDeckUnlockedHandler()
    {
        isSkinLocked = !DeckSkinManager.instance.IsSkinUnlocked(skinData);
        lockedIcon.SetActive(isSkinLocked);

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        TMP_Text selectButtonText = selectButton.GetComponentInChildren<TMP_Text>();

        if (isSkinLocked)
        {
            switch (skinData.unlockMethod)
            {
                case UnlockMethod.Coins:
                case UnlockMethod.Stars:
                    selectButtonText.text = "Locked";
                    break;
                case UnlockMethod.Purchase:
                    selectButtonText.text = "Buy";
                    break;
                default:
                    break;
            }
        }
        else
        {
            selectButtonText.text = DeckSkinManager.instance.CurrentDeckSkin == skinData ? "Selected" : "Select";
        }
    }
}
