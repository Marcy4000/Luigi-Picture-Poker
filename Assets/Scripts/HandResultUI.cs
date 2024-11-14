using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandResultUI : MonoBehaviour
{
    [SerializeField] private GameObject[] sprites;

    private void Start()
    {
        foreach (var sprite in sprites)
        {
            sprite.SetActive(false);
        }
    }

    public void ShowResult(HandType handType)
    {
        foreach (var sprite in sprites)
        {
            sprite.SetActive(false);
        }

        sprites[(int)handType].SetActive(true);
    }

    public void HideResult()
    {
        foreach (var sprite in sprites)
        {
            sprite.SetActive(false);
        }
    }
}
