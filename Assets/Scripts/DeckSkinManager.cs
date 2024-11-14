using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSkinManager : MonoBehaviour
{
    public static DeckSkinManager instance;

    [SerializeField] private List<DeckSkin> deckSkins;
    private List<bool> skinUnlocked = new List<bool>();
    private int currentDeckSkinIndex;

    public DeckSkin CurrentDeckSkin => deckSkins[currentDeckSkinIndex];
    public List<DeckSkin> DeckSkins => deckSkins;

    public event System.Action OnDeckSkinChanged;
    public event System.Action OnDeckSkinUnlocked;

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

        foreach (DeckSkin skin in deckSkins)
        {
            bool unlocked = skin.unlockMethod == UnlockMethod.None ? true : false;
            skinUnlocked.Add(unlocked);
        }
    }

    private void Start()
    {
        GameManager.instance.OnTurnEnded += OnTurnEndedHandler;
    }

    private void OnTurnEndedHandler()
    {
        foreach (DeckSkin skin in deckSkins)
        {
            if (!IsSkinUnlocked(skin))
            {
                switch (skin.unlockMethod)
                {
                    case UnlockMethod.Coins:
                        if (GameManager.instance.Coins >= skin.unlockCost)
                        {
                            UnlockSkin(skin);
                        }
                        break;
                    case UnlockMethod.Stars:
                        if (GameManager.instance.Stars >= skin.unlockCost)
                        {
                            UnlockSkin(skin);
                        }
                        break;
                }
            }
        }
    }

    public void ChangeDeckSkin(int index)
    {
        if (index >= 0 && index < deckSkins.Count)
        {
            currentDeckSkinIndex = index;
            OnDeckSkinChanged?.Invoke();
        }
    }

    public void ChangeDeckSkin(DeckSkin skin)
    {
        int index = deckSkins.IndexOf(skin);
        if (index != -1)
        {
            currentDeckSkinIndex = index;
            OnDeckSkinChanged?.Invoke();
        }
    }

    public bool IsSkinUnlocked(int index)
    {
        return skinUnlocked[index];
    }

    public bool IsSkinUnlocked(DeckSkin skin)
    {
        return skinUnlocked[deckSkins.IndexOf(skin)];
    }

    public void UnlockSkin(int index)
    {
        skinUnlocked[index] = true;
        OnDeckSkinUnlocked?.Invoke();
    }

    public void UnlockSkin(DeckSkin skin)
    {
        skinUnlocked[deckSkins.IndexOf(skin)] = true;
        OnDeckSkinUnlocked?.Invoke();
    }
}
