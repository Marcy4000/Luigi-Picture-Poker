using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameSelector : MonoBehaviour
{
    public static MinigameSelector Instance { get; private set; }

    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject minigameButtonPrefab;
    [SerializeField] private List<Minigame> minigames;
    [SerializeField] private GameObject backButton;

    private bool isOpened = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        foreach (Minigame minigame in minigames)
        {
            GameObject buttonObj = Instantiate(minigameButtonPrefab, buttonContainer);
            MinigameButton minigameButton = buttonObj.GetComponent<MinigameButton>();
            minigameButton.Initialize(minigame, OnMinigameButtonClicked);
        }

        SetOpen(false);
    }

    private void OnMinigameButtonClicked(int sceneBuildIndex)
    {
        StartCoroutine(LoadMinigameScene(sceneBuildIndex));
    }

    private IEnumerator LoadMinigameScene(int sceneBuildIndex)
    {
        SetOpen(false);
        yield return UICircleFade.Instance.DoFadeIn(0.5f).WaitForCompletion();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex);
    }

    public void SetOpen(bool open)
    {
        if (isOpened == open)
            return;

        isOpened = open;
        backButton.SetActive(isOpened);

        float targetX = isOpened ? 0 : panelTransform.rect.height;
        panelTransform.DOAnchorPosY(targetX, 0.5f);
    }
}
