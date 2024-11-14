using UnityEngine;
using UnityEngine.UI;

public class CardValueUI : MonoBehaviour
{
    [SerializeField] private Image valueSprite, background;

    [SerializeField] private Sprite[] backgrounds;

    public void Initialize(CardType cardType)
    {
        DeckSkin deckSkin = DeckSkinManager.instance.CurrentDeckSkin;
        DeckSkin.CardSprites cardSprites = deckSkin.GetSpritesForType(cardType);

        valueSprite.sprite = cardSprites.valueSprite;
        background.sprite = backgrounds[0];
    }

    public void SetBackground(int index)
    {
        if (index >= 0 && index < backgrounds.Length)
            background.sprite = backgrounds[index];
    }
}
