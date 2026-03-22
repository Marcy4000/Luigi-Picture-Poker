using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameMenuOpener : MonoBehaviour
{
    public void OpenMinigameMenu()
    {
        MinigameSelector.Instance.SetOpen(true);
    }

    public void CloseMinigameMenu()
    {
        MinigameSelector.Instance.SetOpen(false);
    }
}
