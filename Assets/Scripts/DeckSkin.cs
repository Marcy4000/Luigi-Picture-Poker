using UnityEngine;

[CreateAssetMenu(fileName = "NewDeckSkin", menuName = "Card Game/Deck Skin")]
public class DeckSkin : ScriptableObject
{
    public Sprite cardBack; // Sprite for the back of the card

    [System.Serializable]
    public struct CardSprites
    {
        public Sprite cardSprite; // First sprite for the card type
        public Sprite valueSprite; // Second sprite for the card type
    }

    public CardSprites starSprites; // Sprites for Star card
    public CardSprites marioSprites; // Sprites for Mario card
    public CardSprites luigiSprites; // Sprites for Luigi card
    public CardSprites flowerSprites; // Sprites for Flower card
    public CardSprites mushroomSprites; // Sprites for Mushroom card
    public CardSprites cloudSprites; // Sprites for Cloud card

    // Method to get the sprites for a specific CardType
    public CardSprites GetSpritesForType(CardType cardType)
    {
        return cardType switch
        {
            CardType.Star => starSprites,
            CardType.Mario => marioSprites,
            CardType.Luigi => luigiSprites,
            CardType.Flower => flowerSprites,
            CardType.Mushroom => mushroomSprites,
            CardType.Cloud => cloudSprites,
            _ => default,
        };
    }
}
