using System.Collections.Generic;
using UnityEngine;

public class CardGenerator
{
    private List<CardType> deck;
    private int currentCardIndex;

    public void InitializeDeck()
    {
        ShuffleDeck();
    }

    // Method to shuffle and reset the deck
    private void ShuffleDeck()
    {
        deck = new List<CardType>();

        for (int i = 0; i <= 5; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                deck.Add((CardType)i);
            }
        }

        // Shuffle the deck
        for (int i = 0; i < deck.Count * 2; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            CardType temp = deck[i % deck.Count];
            deck[i % deck.Count] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        currentCardIndex = 0; // Reset draw position
    }

    // Method to draw a single random card
    public CardType GetRandomCard()
    {
        if (currentCardIndex >= deck.Count)
        {
            ShuffleDeck(); // Reshuffle when out of cards
        }

        return deck[currentCardIndex++];
    }

    // Method to generate a hand of cards (e.g., for initial deal)
    public CardType[] GenerateHand(int handSize = 5)
    {
        CardType[] hand = new CardType[handSize];
        for (int i = 0; i < handSize; i++)
        {
            hand[i] = GetRandomCard();
        }
        return hand;
    }
}