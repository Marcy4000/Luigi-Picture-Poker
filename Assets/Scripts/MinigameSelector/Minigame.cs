using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame", menuName = "Minigame Selector/Minigame")]
public class Minigame : ScriptableObject
{
    public string minigameName;
    public Sprite minigameIcon;
    public int sceneBuildIndex;
}
