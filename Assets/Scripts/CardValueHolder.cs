using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueHolder : MonoBehaviour
{
    [SerializeField] private GameObject valuePrefab;

    private List<GameObject> valueObjects = new List<GameObject>();

    public void Initialize()
    {
        foreach (GameObject valueObject in valueObjects)
        {
            Destroy(valueObject);
        }

        valueObjects.Clear();

        foreach (CardType type in Enum.GetValues(typeof(CardType)))
        {
            CardValueUI valueObject = Instantiate(valuePrefab, transform).GetComponent<CardValueUI>();
            valueObject.Initialize(type);
            valueObjects.Add(valueObject.gameObject);
        }
    }
}
