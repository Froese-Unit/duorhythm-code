using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class MonsterSpawner : MonoBehaviour
{
    public TextMeshProUGUI restText;
    public TextMeshProUGUI startText;
    public TextMeshProUGUI endText;

    [SerializeField] private RankDisplayController rankDisplayController;

    public string ttmonPrefabPath = "Prefab/MonsterTT";
    public string samonPrefabPath = "Prefab/MonsterSA";
    public string mimonPrefabPath = "Prefab/MonsterMi";
    public string baitmonPrefabPath = "Prefab/MonsterBait";
    public float spawnInterval = 12.0f;
    public float spawnDelay = 1.0f;
    public Vector2 spawnRange = new Vector2(5.5f, 2.0f);
    public int totalMonstersToSpawn = 9;
    public bool isDebugMode = false;

    public int currentMonsterID = 0;
    public static int monsterSpawnState = 0;

    private GameObject ttmonPrefab;
    private GameObject samonPrefab;
    private GameObject mimonPrefab;
    private GameObject baitmonPrefab;

    private List<GameObject> monsterPool = new List<GameObject>();
    private GameObject lastMonsterSpawned;

    public CrosshairController crosshairControllerP1;
    public CrosshairController crosshairControllerP2;
    public ScoreManager scoreManager;

    public int numberOfTrials = 3;
    public float restInterval = 3.0f;

    private int currentTrial = 0;
    private int currentBlock = 1;
    private int trialsPerBlock;
    private int totalBlocks;

    private bool isCoroutineRunning = false;
    private bool isTrialStarting = false;

    // Difficulty adjustment variables
    private int previousDifficultyLevel = 0; 
    private int currentDifficultyLevel = 0;
    private float[] difficultySpeeds = { 0f, 0.20f, 0.40f, 0.60f, 0.80f, 1.00f, 1.20f, 1.40f, 1.60f, 1.90f}; 
    private float[] difficultyNoise = { 0f, 0.14f, 0.18f, 0.22f, 0.24f, 0.26f, 0.28f, 0.30f, 0.32f, 0.36f};
    // private int previousBlockScore = 50000;
    private float[] scoreThreholds = { -1500f, 1000f, 3500f, 7500f};


    private NetworkManager networkManager;

    private ProgressBarController progressBarController; 

    public Image levelUpImage;
    public Image levelDownImage;

    private Vector3 originalLevelUpPosition;
    private Vector3 originalLevelDownPosition;

    // New variables to store the score and block where the last level-up happened
    private int lastlevelchangeblockScore = 50000;
    private int lastlevelchangeblock = 1;

    public TextMeshProUGUI nextScoreText; // Reference to the NextScore Text object
    public float targetScore;

    public TextMeshProUGUI distanceToPlanetText;  // Text UI for showing the distance
    private float startingPlanetDistance = 1500000f;
    private float distanceToPlanet = 1500000f;  // Starting distance to the planet (1.5 million meters)
    private bool isHighScoreThreshold;  // Tracks whether any high threshold was triggered

    public Animator boost1Animator;
    public Animator boost2Animator;
    public Animator boost3Animator;
    public Animator slow1Animator;
    public Animator slow2Animator;
    public Animator slow3Animator;
    public Animator farBoost1Animator;
    public Animator farSlow1Animator;
    public Animator midBoost1Animator;
    public Animator midSlow1Animator;

    private Animator currentAnimator;

    public TextMeshProUGUI finalDistanceToPlanetText;
    private void Awake()
    {
        networkManager = GameObject.FindObjectOfType<NetworkManager>();
        if (networkManager == null)
        {
            Debug.LogError("MonsterSpawner: NetworkManager component not found!");
        }
        else
        {
            networkManager.isDebugMode = isDebugMode; // Pass the debug mode flag to NetworkManager
        }

        progressBarController = FindObjectOfType<ProgressBarController>();
        if (progressBarController == null)
        {
            Debug.LogError("ProgressBarController not found in the scene.");
        }

        ttmonPrefab = Resources.Load<GameObject>(ttmonPrefabPath);
        samonPrefab = Resources.Load<GameObject>(samonPrefabPath);
        mimonPrefab = Resources.Load<GameObject>(mimonPrefabPath);
        baitmonPrefab = Resources.Load<GameObject>(baitmonPrefabPath);

        trialsPerBlock = totalMonstersToSpawn;
        totalBlocks = numberOfTrials;
    }

    private void Start()
    {
        // Initialize at the first difficulty level
        currentDifficultyLevel = 0;
        Cursor.visible = false; // Hide the cursor
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

        
        // Disable level change visuals
        levelUpImage.gameObject.SetActive(false);
        levelDownImage.gameObject.SetActive(false);

    // Store original positions
    originalLevelUpPosition = levelUpImage.rectTransform.localPosition;
    originalLevelDownPosition = levelDownImage.rectTransform.localPosition;

    DisableAllAnimators();

        ShowStartText();
        if (isDebugMode)
        {
            HideStartTextAndBeginTrial();
        }
    }

    private void ShowStartText()
    {
        restText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);

        scoreManager.HideScoreUI();
        crosshairControllerP1.HideCrosshair();
        crosshairControllerP2.HideCrosshair();
        startText.gameObject.SetActive(true);
    }

    public void HideStartTextAndBeginTrial()
    {
        startText.gameObject.SetActive(false);
        if (!isTrialStarting)
        {
            StartCoroutine(BeginTrialWithDelay(6.0f));
        }
    }

    private IEnumerator BeginTrialWithDelay(float delay)
    {
        if (isTrialStarting)
        {
            Debug.LogWarning("BeginTrialWithDelay coroutine is already running. Exiting.");
            yield break;
        }

        isTrialStarting = true;

        yield return new WaitForSeconds(delay);
        BeginTrial();

        isTrialStarting = false;
    }

    public void BeginTrial()
    {
        if (isCoroutineRunning)
        {
            Debug.LogWarning("BeginTrial called while another trial is running. Exiting.");
            return;
        }
        rankDisplayController.ResetRankDisplay();
        Debug.Log($"Starting Block {currentBlock}");
        // Adjust difficulty only if not the first block
        if (currentBlock == 1)
        {
            currentDifficultyLevel = 0; // Ensure the first block starts with zero speed
            previousDifficultyLevel = 0;
        }


        ResetGameState();

        currentTrial = 0; // Reset currentTrial at the start of a new block
        lastMonsterSpawned = null;

        CreateBalancedMonsterPool();


        StartCoroutine(SpawnSequence());
 
    }


        private void CheckLevelChange()
    {
        int currentBlockScore = ScoreManager.instance.score; // Get current block score
        int scoreDifference = currentBlockScore - lastlevelchangeblockScore; // Calculate score difference from the last level-up block
        
        previousDifficultyLevel = currentDifficultyLevel;
        isHighScoreThreshold = false;  // Reset the flag before checking

        if (scoreDifference >= scoreThreholds[3])
        {
                isHighScoreThreshold = true; 
            currentDifficultyLevel = Mathf.Min(currentDifficultyLevel + 3, difficultySpeeds.Length - 1);
            lastlevelchangeblockScore = currentBlockScore;  // Update the score only on level-up
            lastlevelchangeblock = currentBlock;            // Update the block where level-up happened
        }
        else if (scoreDifference >= scoreThreholds[2])
        {
                isHighScoreThreshold = true; 
            currentDifficultyLevel = Mathf.Min(currentDifficultyLevel + 2, difficultySpeeds.Length - 1);
            lastlevelchangeblockScore = currentBlockScore;
            lastlevelchangeblock = currentBlock;
        }
        else if (scoreDifference >= scoreThreholds[1])
        {
                isHighScoreThreshold = true; 
            currentDifficultyLevel = Mathf.Min(currentDifficultyLevel + 1, difficultySpeeds.Length - 1);
            lastlevelchangeblockScore = currentBlockScore;
            lastlevelchangeblock = currentBlock;
        }
        else if (scoreDifference <= scoreThreholds[0])
        {
            currentDifficultyLevel = Mathf.Max(currentDifficultyLevel - 1, 1);
            lastlevelchangeblockScore = currentBlockScore;
            lastlevelchangeblock = currentBlock;
        }

        // If no level change occurred, we do not update lastlevelchangeblockScore

        // Show visuals for level up or down
        if (currentDifficultyLevel > previousDifficultyLevel)
        {
            ShowLevelChangeVisual(true); // Show level up animation
        }
        else if (currentDifficultyLevel < previousDifficultyLevel)
        {
            ShowLevelChangeVisual(false); // Show level down animation
        }

        targetScore = lastlevelchangeblockScore + scoreThreholds[1]; 
        UpdateNextScoreText(); 

        Debug.Log($"Current Block Ended: {currentBlock}, Current Score: {currentBlockScore}, Last Level-Up Score: {lastlevelchangeblockScore}, Score Difference: {scoreDifference}");
        Debug.Log($"Previous Difficulty Level: {previousDifficultyLevel}, Current Difficulty Level After Adjustment: {currentDifficultyLevel}");
        Debug.Log($"Difficulty Level adjusted to {currentDifficultyLevel}, Speed: {difficultySpeeds[currentDifficultyLevel]}");
    }

        public void UpdateNextScoreText()
    {
        if (nextScoreText != null)
        {
            nextScoreText.text = $"GET {targetScore} FUEL TO GET A BOOST NEXT!";
        }
    }


    private void CheckLevelChangeFinal()
    {
    int currentBlockScore = ScoreManager.instance.score; // Get current block score
    int scoreDifference = currentBlockScore - lastlevelchangeblockScore; // Calculate score difference from the last level-up block
    
    previousDifficultyLevel = currentDifficultyLevel;
    isHighScoreThreshold = false;  // Reset the flag before checking

    if (scoreDifference >= scoreThreholds[3])
    {
            isHighScoreThreshold = true; 
    }
    else if (scoreDifference >= scoreThreholds[2])
    {
            isHighScoreThreshold = true; 

    }
    else if (scoreDifference >= scoreThreholds[1])
    {
            isHighScoreThreshold = true; 

    }
    else if (scoreDifference <= scoreThreholds[0])
    {
        isHighScoreThreshold = false; 
    }

    }

 
    
    private void ShowLevelChangeVisual(bool isLevelUp)
    {
        if (isLevelUp)
        {
            Debug.Log("Showing Level Up Visual");
            CanvasGroup cg = levelUpImage.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = levelUpImage.gameObject.AddComponent<CanvasGroup>();
            }

            levelUpImage.gameObject.SetActive(true); // Activate level up image
            levelDownImage.gameObject.SetActive(false); // Ensure level down image is hidden

            // Animate: Fade in and move up
            cg.DOFade(1, 0.9f); // Fade to visible
            levelUpImage.rectTransform.DOLocalMoveY(30, 1.0f).SetRelative().SetEase(Ease.OutSine);
        }
        else
        {
            Debug.Log("Showing Level Down Visual");
            CanvasGroup cg = levelDownImage.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = levelDownImage.gameObject.AddComponent<CanvasGroup>();
            }

            levelDownImage.gameObject.SetActive(true); // Activate level down image
            levelUpImage.gameObject.SetActive(false); // Ensure level up image is hidden

            // Animate: Fade in and move down
            cg.DOFade(1, 0.9f); // Fade to visible
            levelDownImage.rectTransform.DOLocalMoveY(-30, 1.0f).SetRelative().SetEase(Ease.OutSine);
        }
    }


    private void ResetLevelVisuals()
    {
        // Reset the position
        levelUpImage.rectTransform.localPosition = originalLevelUpPosition;
        levelDownImage.rectTransform.localPosition = originalLevelDownPosition;

        // Reset CanvasGroup alpha and animations
        CanvasGroup levelUpCg = levelUpImage.GetComponent<CanvasGroup>();
        if (levelUpCg != null)
        {
            levelUpImage.DOKill(); // Stop any ongoing animations
            levelUpCg.alpha = 0; // Reset alpha
        }

        CanvasGroup levelDownCg = levelDownImage.GetComponent<CanvasGroup>();
        if (levelDownCg != null)
        {
            levelDownImage.DOKill(); // Stop any ongoing animations
            levelDownCg.alpha = 0; // Reset alpha
        }
                levelUpImage.gameObject.SetActive(false);
            levelDownImage.gameObject.SetActive(false);
    }


    private void CreateBalancedMonsterPool()
    {
        monsterPool.Clear();

        int monstersPerType = totalMonstersToSpawn / 3;
        int extraMonsters = totalMonstersToSpawn % 3;

        int ttCount = monstersPerType + (extraMonsters > 0 ? 1 : 0);
        int saCount = monstersPerType + (extraMonsters > 1 ? 1 : 0);
        int miCount = monstersPerType;

        List<GameObject> ttMonsters = new List<GameObject>(Enumerable.Repeat(ttmonPrefab, ttCount));
        List<GameObject> saMonsters = new List<GameObject>(Enumerable.Repeat(samonPrefab, saCount));
        List<GameObject> miMonsters = new List<GameObject>(Enumerable.Repeat(mimonPrefab, miCount));

        List<GameObject> allMonsters = new List<GameObject>();
        allMonsters.AddRange(ttMonsters);
        allMonsters.AddRange(saMonsters);
        allMonsters.AddRange(miMonsters);

        allMonsters = allMonsters.OrderBy(a => Random.value).ToList();

        GameObject lastMonster = null;
        for (int i = 0; i < allMonsters.Count; i++)
        {
            GameObject currentMonster = allMonsters[i];
            if (monsterPool.Count > 0 && GetMonsterType(currentMonster) == GetMonsterType(lastMonster))
            {
                int swapIndex = FindNonConsecutiveSwapIndex(i, allMonsters);
                if (swapIndex != -1)
                {
                    currentMonster = allMonsters[swapIndex];
                    allMonsters[swapIndex] = allMonsters[i];
                    allMonsters[i] = currentMonster;
                }
            }
            monsterPool.Add(currentMonster);
            lastMonster = currentMonster;
        }

        Debug.Log("Final monster pool with no consecutive duplicates:");
        LogMonsterPool();
    }

    private void LogMonsterPool()
    {
        for (int i = 0; i < monsterPool.Count; i++)
        {
            Debug.Log($"Monster {i}: {monsterPool[i].name}");
        }
    }

    private int FindNonConsecutiveSwapIndex(int currentIndex, List<GameObject> allMonsters)
    {
        int previousType = currentIndex > 0 ? GetMonsterType(allMonsters[currentIndex - 1]) : -1;
        for (int i = currentIndex + 1; i < allMonsters.Count; i++)
        {
            if (GetMonsterType(allMonsters[i]) != previousType)
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator SpawnSequence()
    {

        if (isCoroutineRunning)
        {
            Debug.LogWarning("SpawnSequence coroutine is already running. Exiting.");
            yield break;
        }

        isCoroutineRunning = true;
        Debug.Log("Starting SpawnSequence coroutine.");

        if (currentTrial >= totalMonstersToSpawn)
        {
            Debug.LogWarning("SpawnSequence already completed. Exiting coroutine.");
            yield break;
        }

        Debug.Log("Starting SpawnSequence coroutine.");

        while (currentTrial < totalMonstersToSpawn)
        {
            
            Debug.Log($"SpawnSequence loop: currentTrial = {currentTrial+1}/{totalMonstersToSpawn}");
            SpawnMonster();
            currentTrial++;
            
            if (!isDebugMode && networkManager != null)
            {
                networkManager.SendTrialInfo(currentBlock, currentTrial, trialsPerBlock, currentMonsterID);
            }

            yield return new WaitForSeconds(spawnInterval);
            DeleteAllMonsters();
            yield return new WaitForSeconds(spawnDelay);
        }

        EndTrial();
        Debug.Log("Ending SpawnSequence coroutine.");
        
        isCoroutineRunning = false;

    }

    private void SpawnMonster()
    {
        if (currentTrial >= totalMonstersToSpawn)
        {
            Debug.LogWarning("Maximum number of monsters spawned. No further spawning.");
            return;
        }

        Debug.Log("Attempting to spawn a monster.");

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            Destroy(monster);
        }

        float offsetX = 2.5f;
        float randomXSide = Random.value < 0.5f ? -1 : 1;
        float spawnX = randomXSide == -1 ? Random.Range(-spawnRange.x, -offsetX) : Random.Range(offsetX, spawnRange.x);
        float spawnY = Random.Range(-spawnRange.y, spawnRange.y);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, -0.1f);

        GameObject monsterToSpawn = monsterPool[currentTrial];
        GameObject newMonster = Instantiate(monsterToSpawn, spawnPosition, Quaternion.identity);
        lastMonsterSpawned = newMonster;

        // Assign movement script and speed
        MonsterMovement monsterMovement = newMonster.AddComponent<MonsterMovement>();
        monsterMovement.SetMoveSpeed(difficultySpeeds[currentDifficultyLevel]); // Set speed based on difficulty
        monsterMovement.SetSteeringIntensity(difficultyNoise[currentDifficultyLevel]); // Set a default steering intensity or adjust as needed

        Vector3 baitmonPosition = new Vector3(-spawnPosition.x, -spawnPosition.y, 0.1f);
        GameObject newBaitmon = Instantiate(baitmonPrefab, baitmonPosition, Quaternion.identity);

        // Assign movement script to baitmon as well
        MonsterMovement baitmonMovement = newBaitmon.AddComponent<MonsterMovement>();
        baitmonMovement.SetMoveSpeed(difficultySpeeds[currentDifficultyLevel]); // Same speed or different, based on gameplay design
        baitmonMovement.SetSteeringIntensity(difficultyNoise[currentDifficultyLevel]); // Set a default steering intensity or adjust as needed

        if (monsterToSpawn == ttmonPrefab)
        {
            currentMonsterID = 11;
            monsterSpawnState = 11;
        }
        else if (monsterToSpawn == samonPrefab)
        {
            currentMonsterID = 21;
            monsterSpawnState = 21;
        }
        else if (monsterToSpawn == mimonPrefab)
        {
            currentMonsterID = 31;
            monsterSpawnState = 31;
        }

            
        Debug.Log($"{newMonster.name.Replace("(Clone)", "").Trim()} spawned at position: {spawnPosition}");
        Debug.Log($"{newBaitmon.name.Replace("(Clone)", "").Trim()} spawned at position: {baitmonPosition}");
    }

    private void DeleteAllMonsters()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            Destroy(monster);
        }

        if (currentMonsterID == 11)
        {
            currentMonsterID = 12;
            monsterSpawnState = 12;
        }
        else if (currentMonsterID == 21)
        {
            currentMonsterID = 22;
            monsterSpawnState = 22;
        }
        else if (currentMonsterID == 31)
        {
            currentMonsterID = 32;
            monsterSpawnState = 32;
        }
    }

    private void ShowEndText()
    {
        crosshairControllerP1.HideCrosshair();
        crosshairControllerP2.HideCrosshair();
        scoreManager.HideScoreUI();

        restText.gameObject.SetActive(false);
        startText.gameObject.SetActive(false);

        if (ScoreManager.instance == null)
        {
            Debug.LogError("ScoreManager instance is null");
        }
        if (rankDisplayController == null)
        {
            Debug.LogError("rankDisplayController is null");
        }

        int finalScore = ScoreManager.instance.score;
        rankDisplayController.DisplayRank(finalScore);

        finalDistanceToPlanetText.gameObject.SetActive(false);

        endText.gameObject.SetActive(true);

        CheckLevelChangeFinal();

        // final distance
        finalDistanceToPlanetText.gameObject.SetActive(true);

        // Ensure CanvasGroup components are attached to both DistanceToPlanet and NextScore
        CanvasGroup distanceCanvasGroup = finalDistanceToPlanetText.GetComponent<CanvasGroup>();
        if (distanceCanvasGroup == null)
        {
            distanceCanvasGroup = finalDistanceToPlanetText.gameObject.AddComponent<CanvasGroup>();
        }
        // Step 2: Fade in DistanceToPlanet and NextScore over 0.5 seconds
        distanceCanvasGroup.alpha = 0;  // Ensure it starts at 0 alpha for fade in

        distanceCanvasGroup.DOFade(1, 1.5f); // Fade in DistanceToPlanet

        float finalScoreEnd = 0f;
        // Step 3: Adjust finalScore if isHighScoreThreshold is true for blocks after block 1
        if (isHighScoreThreshold)
        {

            finalScoreEnd = finalScore * 2;
        }
            else
        {
            finalScoreEnd = finalScore;
        }

        // Step 4: Animate the distance reduction over 1.5 seconds
        float finalDistance = Mathf.Max(0, distanceToPlanet - finalScoreEnd);
        DOTween.To(() => distanceToPlanet, x => {
            distanceToPlanet = x;
                if (finalDistanceToPlanetText != null)
            {
                finalDistanceToPlanetText.text = $"Distance: {distanceToPlanet:N0} m";
            }
        }, finalDistance, 1.5f);

        if (!isDebugMode && networkManager != null)
        {
            networkManager.SendDistanceInfo(distanceToPlanet);
        }

    

    }



    private void EndGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void ResetGameState()
    {
        scoreManager.ResetScore();
        crosshairControllerP1.ResetCrosshairPosition();
        crosshairControllerP2.ResetCrosshairPosition();
        restText.gameObject.SetActive(false);
    }

    private void EndTrial()
    {
        CancelInvoke("SpawnMonster");
        Debug.Log($"Trial {currentTrial} finished!");
    // Stop any ongoing vibration
    StopAllVibrations();
        if (currentBlock == totalBlocks && currentTrial == trialsPerBlock)
        {
            ShowEndText();
            if (!isDebugMode && networkManager != null)
            {
                networkManager.SendTrialInfo(currentBlock, currentTrial, trialsPerBlock, currentMonsterID);
                networkManager.ExperimentEnded();
            }
            Invoke("EndGame", 3.0f);
        }
        else
        {
            ResetLevelVisuals();
            ShowRestText();
            Invoke("BeginNextBlock", restInterval);

        }
    }

    private void BeginNextBlock()
    {
        if (currentBlock < totalBlocks)
        {
            currentBlock++;
            currentTrial = 0; // Reset the trial number for the new block

            BeginTrial();
        }
    }

    private int GetMonsterType(GameObject monster)
    {
        if (monster == ttmonPrefab) return 11;
        if (monster == samonPrefab) return 21;
        if (monster == mimonPrefab) return 31;
        return 0;
    }




// Method to start the full sequence of spaceship animation, distance update, and NextScore display
private void StartFullAnimationSequence(float finalScore)
{
    // Step 1: Ensure all objects start inactive
    distanceToPlanetText.gameObject.SetActive(false);
    nextScoreText.gameObject.SetActive(false);
    DisableAllAnimators();  // Disable all animator objects initially

    // Step 2: Activate the spaceship and play the correct animation based on distance and thresholds
    PlayCorrectSpaceShipAnimation();

    // Step 3: Invoke the next animation (distance and score) after spaceship animation completes
    float animationLength = 3.7f;  // Adjust this based on your animation length
    DOVirtual.DelayedCall(animationLength, () =>
    {
        DisableAllAnimators();
        Debug.Log("Spaceship animation complete, fading in DistanceToPlanet and NextScore.");

        // Step 4: Directly activate and fade in DistanceToPlanet and NextScore
        ActivateAndFadeInDistanceAndNextScore(finalScore);
    });
}

// Method to play the correct spaceship animation based on thresholds and distance
private void PlayCorrectSpaceShipAnimation()
{
    float distancePercentage = distanceToPlanet / startingPlanetDistance;

    // Disable any previous animator
    DisableAllAnimators();

    // Select the correct GameObject (Animator) based on the conditions
    if (distancePercentage < 0.5f)
    {
        // Less than 50% of initial distance
        if (isHighScoreThreshold)
        {
            PlayRandomBoostAnimation();
        }
        else
        {
            PlayRandomSlowAnimation();
        }
    }
    else if (distancePercentage < 0.75f)
    {
        // Less than 75% of initial distance
        if (isHighScoreThreshold)
        {
            currentAnimator = midBoost1Animator;
        }
        else
        {
            currentAnimator = midSlow1Animator;
        }
    }
    else
    {
        // Greater than 75% of initial distance
        if (isHighScoreThreshold)
        {
            currentAnimator = farBoost1Animator;
        }
        else
        {
            currentAnimator = farSlow1Animator;
        }
    }

    // Activate and play the selected GameObject
    if (currentAnimator != null)
    {
        currentAnimator.gameObject.SetActive(true);  // Activate the GameObject
        Debug.Log("Playing animation for " + currentAnimator.gameObject.name);
    }
}

// Method to play a random boost animation
private void PlayRandomBoostAnimation()
{
    int randomBoost = Random.Range(1, 4); // Generates a number between 1 and 3
    switch (randomBoost)
    {
        case 1:
            currentAnimator = boost1Animator;
            break;
        case 2:
            currentAnimator = boost2Animator;
            break;
        case 3:
            currentAnimator = boost3Animator;
            break;
    }

    if (currentAnimator != null)
    {
        currentAnimator.gameObject.SetActive(true);
    }
}

// Method to play a random slow animation
private void PlayRandomSlowAnimation()
{
    int randomSlow = Random.Range(1, 4); // Generates a number between 1 and 3
    switch (randomSlow)
    {
        case 1:
            currentAnimator = slow1Animator;
            break;
        case 2:
            currentAnimator = slow2Animator;
            break;
        case 3:
            currentAnimator = slow3Animator;
            break;
    }

    if (currentAnimator != null)
    {
        currentAnimator.gameObject.SetActive(true);
    }
}

// Method to disable all animators (i.e., all GameObjects that contain animations)
private void DisableAllAnimators()
{
    boost1Animator.gameObject.SetActive(false);
    boost2Animator.gameObject.SetActive(false);
    boost3Animator.gameObject.SetActive(false);
    slow1Animator.gameObject.SetActive(false);
    slow2Animator.gameObject.SetActive(false);
    slow3Animator.gameObject.SetActive(false);
    farBoost1Animator.gameObject.SetActive(false);
    farSlow1Animator.gameObject.SetActive(false);
    midBoost1Animator.gameObject.SetActive(false);
    midSlow1Animator.gameObject.SetActive(false);
}


// Method to activate and fade in DistanceToPlanet and NextScore, and animate the distance reduction
private void ActivateAndFadeInDistanceAndNextScore(float finalScore)
{
    Debug.Log("Activating and fading in DistanceToPlanet and NextScore...");

    // Ensure CanvasGroup components are attached to both DistanceToPlanet and NextScore
    CanvasGroup distanceCanvasGroup = distanceToPlanetText.GetComponent<CanvasGroup>();
    if (distanceCanvasGroup == null)
    {
        distanceCanvasGroup = distanceToPlanetText.gameObject.AddComponent<CanvasGroup>();
    }

    CanvasGroup nextScoreCanvasGroup = nextScoreText.GetComponent<CanvasGroup>();
    if (nextScoreCanvasGroup == null)
    {
        nextScoreCanvasGroup = nextScoreText.gameObject.AddComponent<CanvasGroup>();
    }

    // Step 1: Activate both DistanceToPlanet and NextScore objects
    distanceToPlanetText.gameObject.SetActive(true);
    nextScoreText.gameObject.SetActive(true);

    // Step 2: Fade in DistanceToPlanet and NextScore over 0.5 seconds
    distanceCanvasGroup.alpha = 0;  // Ensure it starts at 0 alpha for fade in
    nextScoreCanvasGroup.alpha = 0; // Ensure it starts at 0 alpha for fade in

    distanceCanvasGroup.DOFade(1, 1.5f); // Fade in DistanceToPlanet
    nextScoreCanvasGroup.DOFade(1, 1.5f); // Fade in NextScore

    // Step 3: Adjust finalScore if isHighScoreThreshold is true for blocks after block 1
    if (currentBlock > 1 && isHighScoreThreshold)
    {
        finalScore *= 2;
    }

    // Step 4: Animate the distance reduction over 1.5 seconds
    float finalDistance = Mathf.Max(0, distanceToPlanet - finalScore);
    DOTween.To(() => distanceToPlanet, x => {
        distanceToPlanet = x;
        UpdateDistanceText();
    }, finalDistance, 1.5f);

    if (!isDebugMode && networkManager != null)
    {
        networkManager.SendDistanceInfo(finalDistance);
    }

}


// Method to update the distance UI text
private void UpdateDistanceText()
{
    if (distanceToPlanetText != null)
    {
        distanceToPlanetText.text = $"Distance: {distanceToPlanet:N0} m";
    }
}






    private void ShowRestText()
    {
        // Update the progress bar with the current block number
        if (progressBarController != null)
        {
            progressBarController.UpdateProgressBar(currentBlock);
        }
        
        crosshairControllerP1.HideCrosshair();
        crosshairControllerP2.HideCrosshair();
        scoreManager.HideScoreUI();

        startText.gameObject.SetActive(false);
        endText.gameObject.SetActive(false);

        if (ScoreManager.instance == null)
        {
            Debug.LogError("ScoreManager instance is null");
        }
        if (rankDisplayController == null)
        {
            Debug.LogError("rankDisplayController is null");
        }

        int finalScore = ScoreManager.instance.score;
        rankDisplayController.DisplayRank(finalScore);

        restText.gameObject.SetActive(true);

        if (currentBlock == 1)
        {
            ShowLevelChangeVisual(true);
            lastlevelchangeblockScore = finalScore;
            currentDifficultyLevel = 3;
            previousDifficultyLevel = 0;
            targetScore = (finalScore/2) + scoreThreholds[1]; 
            UpdateNextScoreText(); 
            Debug.Log("Difficulty level set to 2 for block 2 onwards.");

            StartFullAnimationSequence(finalScore);

        }

        else
        
        {
            CheckLevelChange();
            StartFullAnimationSequence(finalScore);
            
        }

    }
        // Method to stop all vibrations
    private void StopAllVibrations()
    {
        CrosshairController[] crosshairControllers = FindObjectsOfType<CrosshairController>();
        foreach (var controller in crosshairControllers)
        {
            controller.ResetVibration();
        }
    }
}
