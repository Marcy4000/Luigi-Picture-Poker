using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using JSAM;

public class FindLuigiGameManager : MonoBehaviour
{
    public static FindLuigiGameManager Instance { get; private set; }

    [Header("Game Components")]
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private RectTransform gameArea;
    [SerializeField] private Sprite[] characterWantedSprites;
    [SerializeField] private AudioSource characterVoices;
    [SerializeField] private AudioClip[] correctClips;
    [SerializeField] private AudioClip[] wrongClips;
    [SerializeField] private AudioLibrary audioLibrary;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Image wantedImage;

    [Header("Level 3+ Settings")]
    [SerializeField] private int garbledMessCountMin = 20;
    [SerializeField] private int garbledMessCountMax = 40;
    [SerializeField] private int movingGridRowsMin = 3;
    [SerializeField] private int movingGridRowsMax = 6;
    [SerializeField] private int movingGridColsMin = 3;
    [SerializeField] private int movingGridColsMax = 6;
    [SerializeField] private float movingGridSpeedMin = 50f;
    [SerializeField] private float movingGridSpeedMax = 150f;
    [SerializeField] private int topSpawnCountMin = 10;
    [SerializeField] private int topSpawnCountMax = 30;
    [SerializeField] private int itemsPerLineMin = 3; // NEW: min heads per row/column
    [SerializeField] private int itemsPerLineMax = 6; // NEW: max heads per row/column

    [Header("Transition")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float showCorrectDuration = 0.6f;

    private CharacterType targetCharacterType;
    private List<CharacterHead> characterHeads = new List<CharacterHead>();
    private List<GameObject> levelObjects = new List<GameObject>(); // For cleanup

    private float timer = 20f;
    public float Timer => timer;

    private int stars = 0;
    public int Stars => stars;

    private bool isGameOver = false;
    // Controls whether gameplay (and the timer) is active. Paused during transitions and reveal sequences.
    private bool isGameplayActive = false;

    // Enum for random level types from level 3+
    private enum LevelType
    {
        StaticGarbledMess,
        OrganizedMovingPattern,
        MovingGarbledMess,
        TopSpawn
    }

    // Enum for random modifiers from level 10+
    private enum Modifier
    {
        None,
        BlackAndWhite,
        UpsideDown
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        stars = 0;
        timer = 20f;
        isGameOver = false;
        isGameplayActive = false; // start paused until first reveal completes

        if (AudioManager.Instance != null)
            AudioManager.StopAllMusic();
        StartCoroutine(WaitForAudioLoad());

        // Start first level with transition
        StartCoroutine(LevelFlowCoroutine());
    }

    private IEnumerator WaitForAudioLoad()
    {
        yield return new WaitUntil(() => audioLibrary.IsLoaded());
        AudioManager.PlayMusic(DefaultMusic.FindLuigiTheme);
    }

    private void Update()
    {
        if (isGameOver) return;

        // Only decrement timer when gameplay is active
        if (isGameplayActive)
        {
            timer -= Time.deltaTime;
            timerText.text = $"{Mathf.Max(0, timer):F0}";

            if (timer <= 0)
            {
                GameOver();
            }
        }
        else
        {
            // Still update UI display while paused
            timerText.text = $"{Mathf.Max(0, timer):F0}";
        }
    }

    // --- Core Game Loop ---

    private IEnumerator LevelFlowCoroutine()
    {
        if (isGameOver) yield break;

        // Cover screen (pause during transition)
        isGameplayActive = false;
        yield return UICircleFade.Instance.DoFadeIn(fadeDuration).WaitForCompletion();

        // Build level while hidden
        ClearPreviousLevel();
        BuildLevelImmediate();

        // Reveal and start gameplay
        yield return UICircleFade.Instance.DoFadeOut(fadeDuration).WaitForCompletion();
        isGameplayActive = true;
    }

    private void BuildLevelImmediate()
    {
        int currentLevel = stars + 1; // Level 1, 2, 3...

        // 1. Choose target character
        targetCharacterType = (CharacterType)Random.Range(0, System.Enum.GetValues(typeof(CharacterType)).Length);

        // 2. Update UI
        UpdateUI();

        // 3. Determine level type and spawn
        if (currentLevel == 1)
        {
            SpawnStaticGrid(2, 2);
        }
        else if (currentLevel == 2)
        {
            SpawnStaticGrid(5, 5);
        }
        else
        {
            // Random level for 3+
            LevelType randomType = (LevelType)Random.Range(0, System.Enum.GetValues(typeof(LevelType)).Length);
            switch (randomType)
            {
                case LevelType.StaticGarbledMess:
                    SpawnGarbledMess(false);
                    break;
                case LevelType.OrganizedMovingPattern:
                    SpawnOrganizedMovingPattern();
                    break;
                case LevelType.MovingGarbledMess:
                    SpawnGarbledMess(true);
                    break;
                case LevelType.TopSpawn:
                    SpawnTopHidden();
                    break;
            }
        }

        // 4. Apply modifiers for levels 10+
        if (currentLevel >= 10)
        {
            ApplyRandomModifiers();
        }
    }

    private void OnHeadClicked(CharacterType clickedType)
    {
        if (isGameOver) return;

        if (clickedType == targetCharacterType)
        {
            // Start correct handling coroutine
            StartCoroutine(HandleCorrectClickCoroutine());
            // Play correct sound
            if (characterVoices != null && correctClips.Length > 0)
            {
                AudioClip clip = correctClips[(int)clickedType];
                characterVoices.PlayOneShot(clip);
            }
        }
        else
        {
            // Wrong
            timer -= 5f;

            // Play wrong sound
            if (characterVoices != null && wrongClips.Length > 0)
            {
                AudioClip clip = wrongClips[(int)clickedType];
                characterVoices.PlayOneShot(clip);
            }

            // Check for immediate game over
            if (timer <= 0)
            {
                GameOver();
            }

            // Update UI immediately after click
            UpdateUI();
        }
    }

    private IEnumerator HandleCorrectClickCoroutine()
    {
        // Pause gameplay/timer
        isGameplayActive = false;

        // Prevent further input
        foreach (CharacterHead head in characterHeads)
        {
            if (head != null)
            {
                var btn = head.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }
        }

        // Hide wrong heads, keep correct ones visible
        foreach (CharacterHead head in characterHeads)
        {
            if (head == null) continue;
            bool isCorrect = false;
            try
            {
                isCorrect = (head.CharacterType == targetCharacterType);
            }
            catch
            {
                var img = head.GetComponent<Image>();
                if (img != null && wantedImage != null)
                {
                    isCorrect = (img.sprite == wantedImage.sprite);
                }
            }

            head.gameObject.SetActive(isCorrect);
        }

        // Small pause to show correct head(s)
        yield return new WaitForSeconds(showCorrectDuration);

        // Cover and prepare next level (still paused)
        yield return UICircleFade.Instance.DoFadeIn(fadeDuration).WaitForCompletion();

        // Award star and time then start next level
        stars++;
        timer += 5f;
        UpdateUI();

        if (stars % 5 == 0)
        {
            // Every 5 stars, bonus time
            garbledMessCountMax += 5; // increase max count for more challenge
            garbledMessCountMin += 2;  // increase min count for more challenge
            topSpawnCountMax += 5;   // increase max count for more challenge
            topSpawnCountMin += 2;    // increase min count for more challenge
        }

        // Build next level while covered
        ClearPreviousLevel();
        BuildLevelImmediate();

        // Reveal next level and resume gameplay
        yield return UICircleFade.Instance.DoFadeOut(fadeDuration).WaitForCompletion();
        isGameplayActive = true;
    }



    private void GameOver()
    {
        isGameOver = true;
        isGameplayActive = false;
        timer = 0;
        timerText.text = "GAME OVER";
        Debug.Log("Game Over! Final Stars: " + stars);

        // Disable all character buttons
        foreach (CharacterHead head in characterHeads)
        {
            if (head != null)
            {
                var btn = head.GetComponent<Button>();
                if (btn != null) btn.interactable = false;
            }
        }
    }

    // --- UI and Cleanup ---

    private void UpdateUI()
    {
        starsText.text = $"{stars}x";
        timerText.text = $"{Mathf.Max(0, timer):F0}";

        // Set wanted image
        int wantedIndex = (int)targetCharacterType;
        if (wantedIndex >= 0 && wantedIndex < characterWantedSprites.Length)
        {
            wantedImage.sprite = characterWantedSprites[wantedIndex];
        }
    }

    private void ClearPreviousLevel()
    {
        // Reset modifiers from UI
        ResetModifiers();

        // Unsubscribe and destroy all level objects
        foreach (CharacterHead head in characterHeads)
        {
            if (head != null)
            {
                head.OnCharacterClicked -= OnHeadClicked;
            }
        }

        foreach (GameObject obj in levelObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        characterHeads.Clear();
        levelObjects.Clear();
    }

    // --- Spawning Helper ---

    /// <summary>
    /// Spawns a character, initializes it, and subscribes to its click event.
    /// </summary>
    private CharacterHead SpawnCharacter(CharacterType type, Vector2 position, Transform parent)
    {
        GameObject charGO = Instantiate(characterPrefab, parent);
        RectTransform charRT = charGO.GetComponent<RectTransform>();
        charRT.anchoredPosition = position;
        charRT.localScale = Vector3.one;

        CharacterHead head = charGO.GetComponent<CharacterHead>();
        head.Initialize(type);
        head.OnCharacterClicked += OnHeadClicked;
        var btn = head.GetComponent<Button>();
        if (btn != null) btn.interactable = true;

        characterHeads.Add(head);
        // Add the root object to be destroyed (either the head itself or its mover parent)
        if (!levelObjects.Contains(parent.gameObject) && parent != gameArea.transform)
        {
            // This is a child of a mover, add the mover parent
            levelObjects.Add(parent.gameObject);
        }
        else if (parent == gameArea.transform)
        {
            // This is a direct child of the game area, add the head itself
            levelObjects.Add(charGO);
        }

        return head;
    }

    /// <summary>
    /// Gets a random position within the main game area.
    /// </summary>
    private Vector2 GetRandomPositionInGameArea()
    {
        Rect areaRect = gameArea.rect;
        float x = Random.Range(areaRect.xMin, areaRect.xMax);
        float y = Random.Range(areaRect.yMin, areaRect.yMax);
        return new Vector2(x, y);
    }

    // --- Level Spawners ---

    private void SpawnStaticGrid(int rows, int cols)
    {
        int totalCells = rows * cols;
        if (totalCells == 0) return;

        Rect areaRect = gameArea.rect;
        float cellWidth = areaRect.width / cols;
        float cellHeight = areaRect.height / rows;

        List<CharacterType> allTypes = new List<CharacterType>((CharacterType[])System.Enum.GetValues(typeof(CharacterType)));

        if (rows == 2 && cols == 2)
        {
            // Special case: 2x2 grid with all 4 types
            List<CharacterType> typesToSpawn = new List<CharacterType>(allTypes);
            // Shuffle them
            for (int i = 0; i < typesToSpawn.Count; i++)
            {
                CharacterType temp = typesToSpawn[i];
                int randomIndex = Random.Range(i, typesToSpawn.Count);
                typesToSpawn[i] = typesToSpawn[randomIndex];
                typesToSpawn[randomIndex] = temp;
            }

            int typeIndex = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = areaRect.xMin + cellWidth * (c + 0.5f);
                    float y = areaRect.yMax - cellHeight * (r + 0.5f); // yMax is top
                    SpawnCharacter(typesToSpawn[typeIndex], new Vector2(x, y), gameArea.transform);
                    typeIndex++;
                }
            }
        }
        else
        {
            // General grid case (like 5x5)
            int targetRow = Random.Range(0, rows);
            int targetCol = Random.Range(0, cols);
            List<CharacterType> otherTypes = new List<CharacterType>(allTypes);
            otherTypes.Remove(targetCharacterType);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    float x = areaRect.xMin + cellWidth * (c + 0.5f);
                    float y = areaRect.yMax - cellHeight * (r + 0.5f);

                    CharacterType typeToSpawn = (r == targetRow && c == targetCol)
                        ? targetCharacterType
                        : otherTypes[Random.Range(0, otherTypes.Count)];

                    SpawnCharacter(typeToSpawn, new Vector2(x, y), gameArea.transform);
                }
            }
        }
    }

    private void SpawnGarbledMess(bool isMoving)
    {
        // pick a random count within configured min/max (safety: ensure min <= max)
        int minCount = Mathf.Min(garbledMessCountMin, garbledMessCountMax);
        int maxCount = Mathf.Max(garbledMessCountMin, garbledMessCountMax);
        int count = Random.Range(minCount, maxCount + 1);

        // Spawn one target
        CharacterHead targetHead = SpawnCharacter(targetCharacterType, GetRandomPositionInGameArea(), gameArea.transform);
        if (isMoving)
        {
            AddRandomMovement(targetHead);
        }

        List<CharacterType> otherTypes = new List<CharacterType>((CharacterType[])System.Enum.GetValues(typeof(CharacterType)));
        otherTypes.Remove(targetCharacterType);

        // Spawn a bunch of others
        for (int i = 0; i < count - 1; i++)
        {
            CharacterType randomOtherType = otherTypes[Random.Range(0, otherTypes.Count)];
            CharacterHead otherHead = SpawnCharacter(randomOtherType, GetRandomPositionInGameArea(), gameArea.transform);
            if (isMoving)
            {
                AddRandomMovement(otherHead);
            }
        }

        // Shuffle hierarchy for random overlay
        foreach (CharacterHead head in characterHeads)
        {
            head.transform.SetSiblingIndex(Random.Range(0, characterHeads.Count));
        }
    }

    private void AddRandomMovement(CharacterHead head)
    {
        Bouncer bouncer = head.gameObject.AddComponent<Bouncer>();
        bouncer.Initialize(gameArea.rect, Random.Range(50f, 150f));
    }

    private void SpawnOrganizedMovingPattern()
    {
        bool moveHorizontally = Random.Range(0, 2) == 0;

        Rect areaRect = gameArea.rect;

        // pick random rows/cols within configured ranges, ensure min<=max
        int rowsMin = Mathf.Min(movingGridRowsMin, movingGridRowsMax);
        int rowsMax = Mathf.Max(movingGridRowsMin, movingGridRowsMax);
        int colsMin = Mathf.Min(movingGridColsMin, movingGridColsMax);
        int colsMax = Mathf.Max(movingGridColsMin, movingGridColsMax);

        int rows = Random.Range(rowsMin, rowsMax + 1);
        int cols = Random.Range(colsMin, colsMax + 1);

        // pick random itemsPerLine within configured ranges, ensure min<=max and at least 1
        int itemsMin = Mathf.Max(1, Mathf.Min(itemsPerLineMin, itemsPerLineMax));
        int itemsMax = Mathf.Max(1, Mathf.Max(itemsPerLineMin, itemsPerLineMax));
        int itemsPerLineCount = Random.Range(itemsMin, itemsMax + 1);

        // Determine spacing depending on orientation
        float cellWidth = moveHorizontally ? (areaRect.width / itemsPerLineCount) : (areaRect.width / cols);
        float cellHeight = moveHorizontally ? (areaRect.height / rows) : (areaRect.height / itemsPerLineCount);

        // Determine target indices in the chosen layout
        int targetRow, targetCol;
        if (moveHorizontally)
        {
            targetRow = Random.Range(0, rows);
            targetCol = Random.Range(0, itemsPerLineCount);
        }
        else
        {
            targetRow = Random.Range(0, itemsPerLineCount);
            targetCol = Random.Range(0, cols);
        }

        List<CharacterType> otherTypes = new List<CharacterType>((CharacterType[])System.Enum.GetValues(typeof(CharacterType)));
        otherTypes.Remove(targetCharacterType);

        int lineCount = moveHorizontally ? rows : cols;
        int itemsPerLine = itemsPerLineCount;

        for (int i = 0; i < lineCount; i++)
        {
            // Create a parent GameObject for the row/column
            GameObject lineParent = new GameObject(moveHorizontally ? "Row_" + i : "Column_" + i);
            lineParent.transform.SetParent(gameArea, false);
            RectTransform lineRT = lineParent.AddComponent<RectTransform>();
            lineRT.anchorMin = Vector2.zero;
            lineRT.anchorMax = Vector2.one;
            lineRT.sizeDelta = Vector2.zero;
            lineRT.anchoredPosition = Vector2.zero;

            // Add the movement script to this parent
            float speedMin = Mathf.Min(movingGridSpeedMin, movingGridSpeedMax);
            float speedMax = Mathf.Max(movingGridSpeedMin, movingGridSpeedMax);
            float chosenSpeed = Random.Range(speedMin, speedMax);

            PatternMover mover = lineParent.AddComponent<PatternMover>();
            float direction = (Random.Range(0, 2) == 0) ? 1f : -1f; // 1 or -1
            mover.Initialize(gameArea.rect, moveHorizontally, direction * chosenSpeed);

            // Spawn characters as children of this line parent
            for (int j = 0; j < itemsPerLine; j++)
            {
                int r = moveHorizontally ? i : j;
                int c = moveHorizontally ? j : i;

                float x = areaRect.xMin + cellWidth * (c + 0.5f);
                float y = areaRect.yMax - cellHeight * (r + 0.5f);

                CharacterType typeToSpawn = (r == targetRow && c == targetCol)
                    ? targetCharacterType
                    : otherTypes[Random.Range(0, otherTypes.Count)];

                // Spawn character and parent it to the line
                SpawnCharacter(typeToSpawn, new Vector2(x, y), lineParent.transform);
            }
        }
    }

    private void SpawnTopHidden()
    {
        Rect areaRect = gameArea.rect;

        List<CharacterType> otherTypes = new List<CharacterType>((CharacterType[])System.Enum.GetValues(typeof(CharacterType)));
        otherTypes.Remove(targetCharacterType);

        int minCount = Mathf.Min(topSpawnCountMin, topSpawnCountMax);
        int maxCount = Mathf.Max(topSpawnCountMin, topSpawnCountMax);
        int count = Random.Range(minCount, maxCount + 1);

        int targetIndex = Random.Range(0, count);

        // Assume prefab pivot is center. Placing at yMax makes bottom half visible.
        float yPos = areaRect.yMax;

        CharacterHead targetHead = null;

        for (int i = 0; i < count; i++)
        {
            CharacterType typeToSpawn = (i == targetIndex)
                ? targetCharacterType
                : otherTypes[Random.Range(0, otherTypes.Count)];

            float xPos = Random.Range(areaRect.xMin, areaRect.xMax);

            CharacterHead spawned = SpawnCharacter(typeToSpawn, new Vector2(xPos, yPos), gameArea.transform);
            if (i == targetIndex)
            {
                targetHead = spawned;
            }
        }

        // Ensure the target is rendered on top so it's not completely covered by other heads
        if (targetHead != null)
        {
            targetHead.transform.SetAsLastSibling();
        }
    }

    // --- Modifiers ---

    private void ApplyRandomModifiers()
    {
        Modifier mod = (Modifier)Random.Range(0, System.Enum.GetValues(typeof(Modifier)).Length);
        Debug.Log("Applying Modifier: " + mod.ToString());

        switch (mod)
        {
            case Modifier.BlackAndWhite:
                foreach (CharacterHead head in characterHeads)
                {
                    head.SetGrayscale(true);
                }
                break;

            case Modifier.UpsideDown:
                foreach (CharacterHead head in characterHeads)
                {
                    head.transform.localScale = new Vector3(1, -1, 1);
                }
                wantedImage.transform.localScale = new Vector3(1, -1, 1);
                break;
        }
    }

    private void ResetModifiers()
    {
        // Reset materials
        foreach (CharacterHead head in characterHeads)
        {
            if (head != null)
            {
                head.GetComponent<Image>().material = default; // Use default material
                head.transform.localScale = Vector3.one;
            }
        }
        wantedImage.transform.localScale = Vector3.one;
    }
}
