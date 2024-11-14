using JSAM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuigiSfx : MonoBehaviour
{
    public void PlaySound()
    {
        AudioManager.PlaySound(DefaultSounds.Shuffle);
    }
}
