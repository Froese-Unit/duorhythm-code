using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI[] instructionTexts; // Array of instruction texts (Instr1, Instr2, ...)
    public Image triangleButtonCue;
    public GameObject crosshairP1; // The crosshair GameObject for Player 1
    public GameObject crosshairP16; // The crosshair GameObject for Player 1
    public GameObject crosshairBot; // The existing bot crosshair in the scene
    public GameObject leftStickCue; // The Left Stick cue image
    public GameObject crossButtonCue; // The Cross Button cue image
    public GameObject fire2Prefab; // The prefab of the fire2 explosion effect
    public GameObject monsterSAPrefab; // The MonsterSA prefab
    public GameObject monsterTTPrefab; // The MonsterTT prefab
    public GameObject monsterBaitPrefab; // The MonsterBait prefab
    public GameObject monsterMiPrefab; // The MonsterMi prefab
    public TextMeshProUGUI[] instr9Score;

    private int currentStep = 0;
    private GameObject activeFire2Instance; // Track the active instance of fire2
    private GameObject[] activeMonsters = new GameObject[4]; // Track the active instances of monsters
    private GameObject activeMonster; // Track the active monster for Instr6
    private bool isVibrating = false;
    private float vibrationEndTime;

    public TutorialScoreManager tutorialScoreManager;


    private void Start()
    {

        Screen.fullScreen = true;
        Cursor.visible = false; // Hide the cursor
        Cursor.lockState = CursorLockMode.Locked; // Lock the cursor to the center of the screen

        Debug.Log("Initial setup - setting crosshairBot and crosshairP1 inactive");
        crosshairBot.SetActive(false); // Ensure crosshairBot is deactivated at the start
        crosshairP1.SetActive(false);  // Ensure crosshairP1 is deactivated at the start
        leftStickCue.SetActive(false); // Ensure leftStickCue is deactivated at the start
        crossButtonCue.SetActive(false); // Ensure crossButtonCue is deactivated at the start
        tutorialScoreManager.HideScoreUI();

        ShowCurrentInstruction();
    }


    private void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            if (gamepad.buttonNorth.wasPressedThisFrame) // Triangle button
            {
                ProceedToNextInstruction();
            }
            else if (gamepad.dpad.left.wasPressedThisFrame) // D-pad left
            {
                GoToPreviousInstruction();
            }

            // Manage vibration manually
            if (isVibrating && Time.time > vibrationEndTime)
            {
                gamepad.SetMotorSpeeds(0f, 0f);
                isVibrating = false;
            }
        }
    }

    private void ShowCurrentInstruction()
    {
        // Hide all instruction texts first
        foreach (var text in instructionTexts)
        {
            text.gameObject.SetActive(false);
        }

        // Show the current instruction text if within range
        if (currentStep < instructionTexts.Length)
        {
            instructionTexts[currentStep].gameObject.SetActive(true);
            triangleButtonCue.gameObject.SetActive(true); // Show the button cue

            if (currentStep == 2) // Special case for Instr3 to show crosshair and control cues
            {
                Debug.Log("Activating crosshair and cues for Instr3");
                ActivateCrosshairAndCues();
            }
            else
            {
                Debug.Log("Deactivating crosshair and cues");
                DeactivateCrosshairAndCues();
            }

            if (currentStep == 3) // Special case for Instr4 to show explosion effect
            {
                StartExplosionEffect();
            }
            else
            {
                StopExplosionEffect();
            }

            if (currentStep == 4 || currentStep == 12)// Special case for Instr5 to show monster prefabs
            {
                ShowMonsterPrefabs();
            }
            else
            {
                HideMonsterPrefabs();
            }

            if (currentStep == 5) // Special cases for Instr6, Instr7, and Instr8 to spawn monsterSA and show crosshairBot
            {               
                tutorialScoreManager.ResetScore();
                tutorialScoreManager.scoreUI.SetActive(true);
       
                ActivateCrosshairBotAndMonster();
            }
            else if (currentStep == 6) // Special case for Instr8 to spawn TTmon
            {
                tutorialScoreManager.ResetScore();
                tutorialScoreManager.scoreUI.SetActive(true);

                ActivateCrosshairBotAndTTmon();
            }
            else if (currentStep == 7) // Special case for Instr8 to spawn TTmon
            {
                tutorialScoreManager.ResetScore();
                tutorialScoreManager.scoreUI.SetActive(true);

                ActivateCrosshairBotAndMimon();
            }

            else if (currentStep == 9) // Special case for Instr8 to spawn TTmon
            {
                tutorialScoreManager.ResetScore();
                tutorialScoreManager.HideScoreUI();

                ActivateBaitmon();
                
            }
            else
            {
                tutorialScoreManager.ResetScore();
                tutorialScoreManager.HideScoreUI();
                DeactivateCrosshairBotAndMonster();
            }

        }
        else
        {
            EndTutorial();
        }
    }

    private void ProceedToNextInstruction()
    {
        StopAllVibrations();
        currentStep++;
        ShowCurrentInstruction();
    }

    private void GoToPreviousInstruction()
    {
        if (currentStep > 0)
        {
            StopAllVibrations();
            currentStep--;
            ShowCurrentInstruction();
        }
    }

    private void ActivateCrosshairAndCues()
    {
        if (crosshairP1 != null)
        {
            Debug.Log("Setting crosshairP1 active");
            crosshairP1.SetActive(true);
            Debug.Log($"crosshairP1 active state: {crosshairP1.activeSelf}");
            Debug.Log($"crosshairP1 position: {crosshairP1.transform.position}");
        }
        if (leftStickCue != null)
        {
            leftStickCue.SetActive(true);
            Debug.Log($"leftStickCue active state: {leftStickCue.activeSelf}");
        }
        if (crossButtonCue != null)
        {
            crossButtonCue.SetActive(true);
            Debug.Log($"crossButtonCue active state: {crossButtonCue.activeSelf}");
        }
    }

    private void DeactivateCrosshairAndCues()
    {
        if (crosshairP1 != null)
        {
            Debug.Log("Setting crosshairP1 inactive");
            crosshairP1.SetActive(false);
            Debug.Log($"crosshairP1 active state: {crosshairP1.activeSelf}");
        }
        if (leftStickCue != null)
        {
            leftStickCue.SetActive(false);
            Debug.Log($"leftStickCue active state: {leftStickCue.activeSelf}");
        }
        if (crossButtonCue != null)
        {
            crossButtonCue.SetActive(false);
            Debug.Log($"crossButtonCue active state: {crossButtonCue.activeSelf}");
        }
    }

    private void StartExplosionEffect()
    {
        InvokeRepeating(nameof(ShowExplosionEffect), 0f, 1.6f); // Start immediately, repeat every 1.6 seconds
    }

    private void StopExplosionEffect()
    {
        CancelInvoke(nameof(ShowExplosionEffect));
        HideExplosionEffect();
    }

    private void ShowExplosionEffect()
    {
        // Destroy previous instance if it exists
        HideExplosionEffect();

        if (fire2Prefab != null)
        {
            Vector3 position = new Vector3(0, -0.5f, 0); // Example position
            activeFire2Instance = Instantiate(fire2Prefab, position, Quaternion.identity);
            Destroy(activeFire2Instance, 0.6f); // Destroy the instance after the animation duration

            // Trigger vibration with a 0.3s delay
            TriggerVibration(0.1f, 0.3f, 0.1f);
        }
    }

    private void TriggerVibration(float intensity, float delay, float duration)
    {
        StartCoroutine(VibrationRoutine(intensity, delay, duration));
    }

    private IEnumerator VibrationRoutine(float intensity, float delay, float duration)
    {
        // Wait for the delay before starting vibration
        yield return new WaitForSeconds(delay);

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(intensity, intensity);
            isVibrating = true;
            vibrationEndTime = Time.time + duration;
        }
    }

    private void HideExplosionEffect()
    {
        if (activeFire2Instance != null)
        {
            Destroy(activeFire2Instance);
            activeFire2Instance = null;
        }
    }

    private void ShowMonsterPrefabs()
    {
        HideMonsterPrefabs(); // Clear any existing monsters

        Vector3 startPosition = new Vector3(-6.00f, -1.0f, 0); // Starting position for the first monster
        float spacing = 4.00f; // Spacing between each monster
        Vector3 scale = new Vector3(3f, 3f, 1); // Scale down to 0.8

        activeMonsters[2] = Instantiate(monsterBaitPrefab, startPosition , Quaternion.identity);
        activeMonsters[2].transform.localScale = scale;

        activeMonsters[0] = Instantiate(monsterSAPrefab, startPosition + new Vector3(spacing * 1, 0, 0), Quaternion.identity);
        activeMonsters[0].transform.localScale = scale;

        activeMonsters[1] = Instantiate(monsterTTPrefab, startPosition + new Vector3(spacing * 2, 0, 0), Quaternion.identity);
        activeMonsters[1].transform.localScale = scale;

        activeMonsters[3] = Instantiate(monsterMiPrefab, startPosition + new Vector3(spacing * 3, 0, 0), Quaternion.identity);
        activeMonsters[3].transform.localScale = scale;
    }

    private void HideMonsterPrefabs()
    {
        foreach (var monster in activeMonsters)
        {
            if (monster != null)
            {
                Destroy(monster);
            }
        }
    }

    private void ActivateCrosshairBotAndMonster()
    {
        DeactivateCrosshairBotAndMonster(); // Clear any existing monsterSA and crosshairs

        Vector3 position = new Vector3(0f, -0.5f, 0); // Position for monsterSA
        activeMonster = Instantiate(monsterSAPrefab, position, Quaternion.identity);

        if (crosshairBot != null)
        {
            crosshairBot.SetActive(true);

            // Calculate the boundaries of the spawned monsterSA
            var monsterBounds = activeMonster.GetComponent<Renderer>().bounds;
            var min = new Vector2(monsterBounds.min.x, monsterBounds.min.y);
            var max = new Vector2(monsterBounds.max.x, monsterBounds.max.y);

            // Set boundaries for CrosshairBot
            var botController = crosshairBot.GetComponent<CrosshairBotController>();
            if (botController != null)
            {
                botController.SetBoundaries(min, max);
                botController.useDynamicReloadTime = false; // Disable dynamic reload time for SAmon
                botController.useRandomReloadTime = false;
                           botController.StartMovingAndShooting(new Vector3(1.5f, 1.0f, 0)); // Pass the target position
      
            }
        }

        if (crosshairP16 != null)
        {
            Debug.Log("Setting crosshairP1 active in ActivateCrosshairBotAndMonster");
            crosshairP16.SetActive(true);
        }
    }
    
    private void DeactivateCrosshairBotAndMonster()
    {
        if (activeMonster != null)
        {
            Destroy(activeMonster);
            activeMonster = null;
        }

        if (crosshairBot != null)
        {
            var botController = crosshairBot.GetComponent<CrosshairBotController>();
            if (botController != null)
            {
                botController.StopMovingAndShooting();
            }
            crosshairBot.SetActive(false); // Deactivate the bot crosshair
        }

        if (crosshairP16 != null)
        {
            crosshairP16.SetActive(false);
        }
    }


    private void ActivateCrosshairBotAndTTmon()
{
    DeactivateCrosshairBotAndMonster(); // Clear any existing monsterSA and crosshairs

    Vector3 position = new Vector3(0f, -0.5f, 0); // Position for TTmon
    activeMonster = Instantiate(monsterTTPrefab, position, Quaternion.identity);

    if (crosshairBot != null)
    {
        crosshairBot.SetActive(true);

        // Calculate the boundaries of the spawned TTmon
        var monsterBounds = activeMonster.GetComponent<Renderer>().bounds;
        var min = new Vector2(monsterBounds.min.x, monsterBounds.min.y);
        var max = new Vector2(monsterBounds.max.x, monsterBounds.max.y);

        // Set boundaries for CrosshairBot
        var botController = crosshairBot.GetComponent<CrosshairBotController>();
        if (botController != null)
        {
            botController.SetBoundaries(min, max);
            botController.useDynamicReloadTime = true; // Enable dynamic reload time
            botController.useRandomReloadTime = false;
            botController.StartMovingAndShooting(new Vector3(-1.5f, -0.5f)); // Pass the target position
        }
    }

    if (crosshairP1 != null)
    {
        Debug.Log("Setting crosshairP1 active in ActivateCrosshairBotAndTTmon");
        crosshairP16.SetActive(true);
    }
}

private void ActivateCrosshairBotAndMimon()
{
    DeactivateCrosshairBotAndMonster(); // Clear any existing monsterSA and crosshairs

    Vector3 position = new Vector3(0f, -0.5f, 0); // Position for TTmon
    activeMonster = Instantiate(monsterMiPrefab, position, Quaternion.identity);

    if (crosshairBot != null)
    {
        crosshairBot.SetActive(true);

        // Calculate the boundaries of the spawned TTmon
        var monsterBounds = activeMonster.GetComponent<Renderer>().bounds;
        var min = new Vector2(monsterBounds.min.x, monsterBounds.min.y);
        var max = new Vector2(monsterBounds.max.x, monsterBounds.max.y);

        // Set boundaries for CrosshairBot
        var botController = crosshairBot.GetComponent<CrosshairBotController>();
        if (botController != null)
        {
            botController.SetBoundaries(min, max);
            botController.useDynamicReloadTime = false;
            botController.useRandomReloadTime = true; // Enable dynamic reload time
            botController.StartMovingAndShooting(new Vector3(0.5f, -1.5f)); // Pass the target position
        }
    }

    if (crosshairP1 != null)
    {
        Debug.Log("Setting crosshairP1 active in ActivateCrosshairBotAndTTmon");
        crosshairP16.SetActive(true);
    }
}

private void ActivateBaitmon()
{
    DeactivateCrosshairBotAndMonster(); // Clear any existing monsterSA and crosshairs

    Vector3 position = new Vector3(0f, -0.5f, 0); // Position for TTmon
    activeMonster = Instantiate(monsterBaitPrefab, position, Quaternion.identity);
}


    private void StopAllVibrations()
    {
        var currentGamepad = Gamepad.current;
        if (currentGamepad != null)
        {
            currentGamepad.SetMotorSpeeds(0f, 0f);
        }
        isVibrating = false;
    }

    private void EndTutorial()
    {
        // Hide all instructions and button cue
        foreach (var text in instructionTexts)
        {
            text.gameObject.SetActive(false);
        }
        triangleButtonCue.gameObject.SetActive(false);

        // Hide crosshair and control cues
        DeactivateCrosshairAndCues();
        DeactivateCrosshairBotAndMonster();

        StopExplosionEffect();
        HideMonsterPrefabs();

        StopAllVibrations();

        // Logic to end the tutorial, e.g., return to main menu or exit game
        Debug.Log("Tutorial Ended");
        Application.Quit(); // Quit the application

        // If running in the Unity editor, use the following instead to stop play mode:
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
