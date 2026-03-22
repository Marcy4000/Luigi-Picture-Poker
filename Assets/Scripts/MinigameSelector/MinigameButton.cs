using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameButton : MonoBehaviour
{
    [SerializeField] private Image minigameImage;
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private Button buttonComponent;
    private int minigameSceneIndex;

    public void Initialize(Minigame minigame, Action<int> onClickAction)
    {
        minigameImage.sprite = minigame.minigameIcon;
        minigameSceneIndex = minigame.sceneBuildIndex;
        buttonComponent.onClick.AddListener(() => onClickAction(minigameSceneIndex));
        selectedFrame.SetActive(SceneManager.GetActiveScene().buildIndex == minigameSceneIndex);

        SceneManager.activeSceneChanged += (oldScene, newScene) =>
        {
            selectedFrame.SetActive(newScene.buildIndex == minigameSceneIndex);
        };
    }
}
