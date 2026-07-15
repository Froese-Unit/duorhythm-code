using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairBotController : MonoBehaviour
{

    public float speed = 5.0f;
    public GameObject explosionEffect; // Drag your explosion effect prefab here
    public float reloadTime = 1.0f;  // Time before player can shoot again

    private float lastShotTime;  // Time the last shot was taken
    private Vector2 boundaryMin;
    private Vector2 boundaryMax;
    private bool shouldMoveAndShoot = false;
    private Vector3 targetPosition;
    private bool isVibrating = false;
    private float vibrationEndTime;

    private static Gamepad gamepad;
    public bool useDynamicReloadTime = false; // Flag to use dynamic reload time
    public bool useRandomReloadTime = false; // Flag to use dynamic reload time


    float lastBotShotTime = 0f; // Track the time of the bot's last shot
    float minBotShootInterval = 0.7f; // Minimum interval between bot shots


    // Instr6 Variables
    public Image accuracyBar;  // Reference to the accuracy bar
    public Image boundaryLine; // Reference to the boundary line image
    public float shotWindow = 0.17f;  // Optimal shot window (0.17s)
    public float barDuration = 0.7f;  // Total time for the bar to fill
    private float barStartTime;  // Start time of the bar fill
    private bool isBarFilling = false;  // Track if the bar is actively filling

    // Instr7 Variables
    public Image accuracyBar2;  // Reference to the accuracy bar for Instr7
    public Image boundaryStart; // Reference to BoundaryStart
    public Image boundaryEnd;   // Reference to BoundaryEnd
    public float shotWindowStart = 0.35f;  // Start of the optimal shot window
    public float shotWindowEnd = 0.65f;    // End of the optimal shot window
    private bool isBarFillingInstr7 = false;  // Track if the bar for Instr7 is actively filling

    private void Start()
    {


       // Ensure bar for Instr6 is always visible
        accuracyBar.fillAmount = 0;
        accuracyBar.color = Color.green;
        isBarFilling = false;  // Bar filling is only active when the bot shoots

        // Ensure bar for Instr7 is always visible
        accuracyBar2.fillAmount = 0;
        accuracyBar2.color = Color.red; // Default to red until inside the window
        isBarFillingInstr7 = false;  // Bar filling is only active when the bot shoots

        // Ensure the boundary lines are placed correctly for both Instr6 and Instr7
        PlaceBoundaryLine();
        PlaceBoundaryLinesInstr7();


        lastShotTime = -reloadTime;  // Initialize to allow shooting immediately

        // Initialize the gamepad variable
        gamepad = Gamepad.current;

        if (gamepad == null)
        {
            Debug.LogError("No gamepad connected.");
        }
    }
    private void PlaceBoundaryLine()
    {
        // Ensure the boundary line is placed relative to the parent container (accBar)
        if (boundaryLine != null)
        {
            // Get the RectTransform of the boundary and the parent container (accBar)
            RectTransform boundaryRT = boundaryLine.GetComponent<RectTransform>();
            RectTransform parentRT = accuracyBar.GetComponentInParent<RectTransform>(); // Get the parent container (accBar)

            // Calculate the boundary position based on shotWindow and parent container width
            float boundaryPosition = (shotWindow / barDuration) * parentRT.rect.width;

            // Set the boundary's anchored position (X) to boundaryPosition
            boundaryRT.anchoredPosition = new Vector2(boundaryPosition - parentRT.rect.width * 0.5f, boundaryRT.anchoredPosition.y);
        }
    }


private void PlaceBoundaryLinesInstr7()
{
    // Ensure the boundary lines are placed relative to the parent container (accBar)
    if (boundaryStart != null && boundaryEnd != null)
    {
        // Get the RectTransform of the boundaries and the parent container (accBar)
        RectTransform boundaryStartRT = boundaryStart.GetComponent<RectTransform>();
        RectTransform boundaryEndRT = boundaryEnd.GetComponent<RectTransform>();
        RectTransform parentRT = accuracyBar.GetComponentInParent<RectTransform>(); // Get the parent container (accBar)

        // Calculate the positions for both boundaries based on shotWindowStart and shotWindowEnd
        float boundaryStartPosition = (shotWindowStart / barDuration) * parentRT.rect.width;
        float boundaryEndPosition = (shotWindowEnd / barDuration) * parentRT.rect.width;

        // Set the boundaryStart's anchored position (X) to boundaryStartPosition
        boundaryStartRT.anchoredPosition = new Vector2(boundaryStartPosition - parentRT.rect.width * 0.5f, boundaryStartRT.anchoredPosition.y);

        // Set the boundaryEnd's anchored position (X) to boundaryEndPosition
        boundaryEndRT.anchoredPosition = new Vector2(boundaryEndPosition - parentRT.rect.width * 0.5f, boundaryEndRT.anchoredPosition.y);
    }
}



    private void Update()
    {
        if (shouldMoveAndShoot)
        {
            // Move to the target position
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // If reached the target position, stop moving and start shooting
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f && Time.time - lastShotTime >= reloadTime)
            {
                Shoot();
            }
        }

        // Manage vibration manually
        if (isVibrating && Time.time > vibrationEndTime)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
            isVibrating = false;
        }
            // Update the accuracy bar if it's active
        // Update the accuracy bar if it's active (Instr6)
        if (isBarFilling)
        {
            UpdateAccuracyBar();
        }

        // Update the accuracy bar if it's active (Instr7)
        if (isBarFillingInstr7)
        {
            UpdateAccuracyBarInstr7();
        }
    }

    public void SetBoundaries(Vector2 min, Vector2 max)
    {
        boundaryMin = min;
        boundaryMax = max;
    }

    public void StartMovingAndShooting(Vector3 targetPos)
    {
        targetPosition = targetPos; // Set the target position
        shouldMoveAndShoot = true;
        if (useDynamicReloadTime)
        {
            StartCoroutine(DynamicReloadTimeRoutine()); // Start the coroutine to handle dynamic reload time
        }
        if (useRandomReloadTime)
        {
            StartCoroutine(RandomReloadTimeRoutine()); // Start the coroutine to handle dynamic reload time
        }

    }

    public void StopMovingAndShooting()
    {
        shouldMoveAndShoot = false;
        useDynamicReloadTime = false;
    }

    private void Shoot()
    {
        if (Time.time - lastShotTime >= reloadTime)
        {
            Vector3 explosionPosition = new Vector3(transform.position.x, transform.position.y, 0);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero);

            if (explosionEffect != null)
            {
                GameObject explosion = Instantiate(explosionEffect, explosionPosition, Quaternion.identity);
                Destroy(explosion, 0.6f); // Destroy the instance after the animation duration
            }
            lastShotTime = Time.time;

            
            if (hit.collider != null)
            {
                Debug.Log("Raycast hit: " + hit.collider.name);
            }
            else
            {
                Debug.Log("Raycast did not hit anything.");
            }

            if (hit.collider != null && hit.collider.CompareTag("Monster"))
            {
                HandleMonsterShot(hit.collider, "Player2");
            }

            // Log the time of shooting
            Debug.Log($"Shot fired at: {Time.time}");

            // Trigger vibration with a 0.3s delay
            TriggerVibration(0.1f, 0.3f, 0.1f);
        }
        // Check if accuracyBar2 (Instr7) is active in the scene
        if (accuracyBar2 != null && accuracyBar2.gameObject.activeInHierarchy)
        {
            StartAccuracyBarInstr7();  // For Instr7
        }
        else
        {
            StartAccuracyBar();  // For Instr6
        }
    }


    private void UpdateAccuracyBar()
    {
        float timeElapsed = Time.time - barStartTime;
        float progress = timeElapsed / barDuration;

        if (timeElapsed <= barDuration)
        {
            // Update the fill amount of the bar
            accuracyBar.fillAmount = progress;

            // Change color after 0.17s (shot window for Instr6)
            if (timeElapsed > shotWindow)
            {
                accuracyBar.color = new Color(1f, 0f, 0f, 1f);  // Full red, fully opaque
            }
            else
            {
                accuracyBar.color = new Color(0f, 1f, 0f, 1f);  // Full green, fully opaque
            }
        }
        else
        {
            // Reset the bar filling logic after the duration is reached
            isBarFilling = false;
            accuracyBar.fillAmount = 0;  // Optionally reset the fill amount
        }
    }

    private void UpdateAccuracyBarInstr7()
    {
        float timeElapsed = Time.time - barStartTime;
        float progress = timeElapsed / barDuration;

        if (timeElapsed <= barDuration)
        {
            // Update the fill amount of the bar for Instr7 (accBar2)
            accuracyBar2.fillAmount = progress;

            // Change color based on the time window for Instr7
            if (timeElapsed >= shotWindowStart && timeElapsed <= shotWindowEnd)
            {
                accuracyBar2.color = new Color(0f, 1f, 0f, 1f);  // Full green, fully opaque
            }
            else
            {
                accuracyBar2.color = new Color(1f, 0f, 0f, 1f);  // Full red, fully opaque
            }
        }
        else
        {
            // Reset the bar filling logic after the duration is reached
            isBarFillingInstr7 = false;
            accuracyBar2.fillAmount = 0;  // Optionally reset the fill amount
        }
    }



    private void StartAccuracyBar()
    {
        barStartTime = Time.time;
        isBarFilling = true;
        accuracyBar.fillAmount = 0;  // Reset fill amount
        accuracyBar.color = new Color(0f, 1f, 0f, 1f);  // Start with green color (fully opaque)
    }

    private void StartAccuracyBarInstr7()
    {
        barStartTime = Time.time;
        isBarFillingInstr7 = true;
        accuracyBar2.fillAmount = 0;  // Reset fill amount
        accuracyBar2.color = new Color(1f, 0f, 0f, 1f);  // Start with red color (fully opaque)
    }

     private void HandleMonsterShot(Collider2D collider, string playerName)
    {
        TutorialTTmon monsterScriptTT = collider.GetComponent<TutorialTTmon>();
                TutorialSAmon monsterScriptSA = collider.GetComponent<TutorialSAmon>();
        TutorialMimon monsterScriptMimon = collider.GetComponent<TutorialMimon>();
        Baitmon monsterScriptBaitmon = collider.GetComponent<Baitmon>();

        if (monsterScriptTT != null)
        {
            monsterScriptTT.ShotByPlayer(playerName);
            Debug.Log($"Shot hit TTmon by {playerName}");
        }
        else if (monsterScriptSA != null)
        {
            monsterScriptSA.ShotByPlayer(playerName);
            Debug.Log($"Shot hit SAmon by {playerName}");
        }
        else if (monsterScriptMimon != null)
        {
            monsterScriptMimon.ShotByPlayer(playerName);
            Debug.Log($"Shot hit Mimon by {playerName}");
        }
        else if (monsterScriptBaitmon != null)
        {
            monsterScriptBaitmon.ShotByPlayer(playerName);
            Debug.Log($"Shot hit Baitmon by {playerName}");
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

        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(intensity, intensity);
            isVibrating = true;
            vibrationEndTime = Time.time + duration;
        }
    }

        public void SetReloadTime(float newReloadTime)
    {
        reloadTime = newReloadTime;
    }

    private IEnumerator DynamicReloadTimeRoutine()
    {
        while (shouldMoveAndShoot)
        {
            yield return new WaitForSeconds(reloadTime); // Wait for the current reload time

            float shotTimeDiff = Time.time - CrosshairControllerSimple.playerLastShotTime;
            if (Time.time - lastBotShotTime >= minBotShootInterval)
            {
                if (shotTimeDiff >= 0.35f && shotTimeDiff <= 0.65f)
                {
                    reloadTime = Random.Range( shotTimeDiff + 0.45f, shotTimeDiff + 0.55f);
                }
                else
                {
                    reloadTime = 1.0f;
                }
            }
            else
            {
                reloadTime = 0.8f;
            }
            // Register the bot's shot
            lastBotShotTime = Time.time; // Update the time of the bot's shot

        }
    }

    private IEnumerator RandomReloadTimeRoutine()
    {
        while (shouldMoveAndShoot)
        {
            yield return new WaitForSeconds(reloadTime); // Wait for the current reload time

            reloadTime = Random.Range(0.701f, 0.9f);

        }
    }

}
