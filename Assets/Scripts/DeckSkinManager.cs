using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSkinManager : MonoBehaviour
{
    public static DeckSkinManager instance;

    [SerializeField] private List<DeckSkin> deckSkins;
    private int currentDeckSkinIndex;

    public DeckSkin CurrentDeckSkin => deckSkins[currentDeckSkinIndex];
    public List<DeckSkin> DeckSkins => deckSkins;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeDeckSkin(int index)
    {
        if (index >= 0 && index < deckSkins.Count)
        {
            currentDeckSkinIndex = index;
        }
    }
}
