using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterType
{
    Luigi,
    Mario,
    Yoshi,
    Wario
}

public class CharacterHead : MonoBehaviour
{
    [SerializeField] private Sprite[] characterSprites;
    [SerializeField] private Sprite[] characterSpritesGrayscale;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button characterButton;

    private CharacterType characterType;

    public CharacterType CharacterType => characterType;
    public event System.Action<CharacterType> OnCharacterClicked;

    private void Start()
    {
        characterImage.alphaHitTestMinimumThreshold = 0.1f;
        characterButton.onClick.AddListener(() => OnCharacterClicked?.Invoke(characterType));
    }

    public void Initialize(CharacterType type)
    {
        characterType = type;
        UpdateCharacterImage();
    }

    private void UpdateCharacterImage()
    {
        int index = (int)characterType;
        if (index >= 0 && index < characterSprites.Length)
        {
            characterImage.sprite = characterSprites[index];
        }
        else
        {
            Debug.LogWarning("Character type index out of range.");
        }
    }

    public void SetGrayscale(bool isGrayscale)
    {
        int index = (int)characterType;
        if (isGrayscale)
        {
            if (index >= 0 && index < characterSpritesGrayscale.Length)
            {
                characterImage.sprite = characterSpritesGrayscale[index];
            }
            else
            {
                Debug.LogWarning("Character type index out of range for grayscale.");
            }
        }
        else
        {
            UpdateCharacterImage();
        }
    }
}
