using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Linq;

public class CrosshairController : MonoBehaviour
{

    public float speed = 5.0f;
    public bool isP1 = true;  // Set this for Player 1 and unset for Player 2 in the Inspector
    public bool isVisible = true;  // For toggling visibility
    public GameObject explosionEffect; // Drag your explosion effect prefab here
    public float reloadTime = 0.7f;  // Time before player can shoot again
    public GameObject square;  // Drag the Square object here in the inspector
    public Image reloadBar;  // Drag the ReloadBar image here in the inspector

    private Vector2 boundary;
    private float halfWidth;
    private float halfHeight;
    private SpriteRenderer spriteRenderer;
    private float lastShotTime;  // Time the last shot was taken

    public static bool hasP1Shot = false;
    public static bool hasP2Shot = false;

    public static int P1shotsuccess = 0;
    public static int P2shotsuccess = 0;

    private bool shootInput;
    private Vector2 moveInput;

    private Gamepad assignedGamepad;
    private Gamepad otherGamepad;

    private bool isVibrating = false;  // Flag to track if a vibration is ongoing
    
    private void Awake()
    {
        shootInput = false;
        
    // Assign the correct Gamepad to this player
    if (isP1)
    {
        // P1 should use the controller with the name "DualSenseGamepadHID1"
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.name == "DualSenseGamepadHID1")
            {
                assignedGamepad = gamepad;
            }
            else if (gamepad.name == "DualSenseGamepadHID")
            {
                otherGamepad = gamepad;
            }
        }
    }
    else
    {
        // P2 should use the controller with the name "DualSenseGamepadHID"
        foreach (var gamepad in Gamepad.all)
        {
            if (gamepad.name == "DualSenseGamepadHID")
            {
                assignedGamepad = gamepad;
            }
            else if (gamepad.name == "DualSenseGamepadHID1")
            {
                otherGamepad = gamepad;
            }
        }
    }

    }


    public void HideCrosshair()
    {
        this.gameObject.SetActive(false);
    }

    public void ResetCrosshairPosition()
    {
        if (isP1)
        {
            transform.position = new Vector3(-0.5f, 0, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(0.5f, 0, transform.position.z);
        }

        this.gameObject.SetActive(true); // This will also re-enable the crosshair
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastShotTime = -reloadTime;  // Initialize to allow shooting immediately
        boundary = new Vector2(square.transform.localScale.x / 2, square.transform.localScale.y / 2);

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        halfWidth = sprite.rect.width / sprite.pixelsPerUnit / 2;
        halfHeight = sprite.rect.height / sprite.pixelsPerUnit / 2;

        if (reloadBar != null)
        {
            reloadBar.type = Image.Type.Filled;
            reloadBar.fillMethod = Image.FillMethod.Horizontal;
            reloadBar.fillOrigin = (int)Image.OriginHorizontal.Left;
            reloadBar.fillAmount = 0f;
            reloadBar.gameObject.SetActive(false); // Ensure it starts as inactive
        }
        else
        {
            Debug.LogError("ReloadBar is not assigned in the inspector.");
        }
    }

    void Update()
    {
        if (assignedGamepad != null)
        {
            // Read the input from the assigned gamepad
            moveInput = new Vector2(assignedGamepad.leftStick.x.ReadValue(), assignedGamepad.leftStick.y.ReadValue());
            // shootInput = assignedGamepad.buttonSouth.isPressed;
            shootInput = assignedGamepad.rightShoulder.isPressed;

            Vector3 movement = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;
            transform.position += movement;

            if (reloadBar != null)
            {
                if (Time.time - lastShotTime < reloadTime)
                {
                    float reloadProgress = (Time.time - lastShotTime) / reloadTime;
                    reloadBar.fillAmount = reloadProgress;
                    reloadBar.gameObject.SetActive(true);
                }
                else
                {
                    reloadBar.fillAmount = 0f;
                    reloadBar.gameObject.SetActive(false);
                }
            }

            if (Time.time - lastShotTime >= reloadTime && shootInput)
            {
                Debug.Log(isP1 ? "Player 1 shot!" : "Player 2 shot!");
                Shoot();
                lastShotTime = Time.time;
                if (isP1) hasP1Shot = true;
                else hasP2Shot = true;

                if (reloadBar != null)
                {
                    reloadBar.fillAmount = 0f;
                    reloadBar.gameObject.SetActive(true);  // Show reload bar when shooting
                }
            }

            spriteRenderer.enabled = isVisible;  

            float clampedX = Mathf.Clamp(transform.position.x, -boundary.x + halfWidth, boundary.x - halfWidth);
            float clampedY = Mathf.Clamp(transform.position.y, -boundary.y + halfHeight, boundary.y - halfHeight);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
        else
        {
            Debug.LogError($"No gamepad assigned to {(isP1 ? "Player 1" : "Player 2")}");
        }
    }

 void Shoot()
{
    Vector3 explosionPosition = new Vector3(transform.position.x, transform.position.y, 0);
    GameObject explosion = Instantiate(explosionEffect, explosionPosition, Quaternion.identity);

    // Use RaycastAll to get all objects under the crosshair
    RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.zero);

    if (hits.Length > 0)
    {
        // Sort the hits by sortingOrder in descending order to get the topmost object
        var sortedHits = hits.OrderByDescending(hit => hit.collider.gameObject.GetComponent<SpriteRenderer>().sortingOrder).ToArray();

        // The first element in sortedHits is the topmost object
        RaycastHit2D topHit = sortedHits[0];

        Debug.Log("Topmost hit object: " + topHit.collider.name);

        // Check if the topmost hit is a "Monster" and proceed with the existing logic
        if (topHit.collider.CompareTag("Monster"))
        {
            TTmon monsterScriptTT = topHit.collider.GetComponent<TTmon>();
            SAmon monsterScriptSA = topHit.collider.GetComponent<SAmon>();
            Mimon monsterScriptMimon = topHit.collider.GetComponent<Mimon>();
            Baitmon monsterScriptBaitmon = topHit.collider.GetComponent<Baitmon>();

            string shootingPlayer = isP1 ? "Player1" : "Player2";

            if (monsterScriptTT != null)
            {
                monsterScriptTT.ShotByPlayer(shootingPlayer);
                if (isP1)
                {
                    P1shotsuccess = 1;
                }
                else
                {
                    P2shotsuccess = 1;
                }
            }
            else if (monsterScriptSA != null)
            {
                monsterScriptSA.ShotByPlayer(shootingPlayer);
                if (isP1)
                {
                    P1shotsuccess = 2;
                }
                else
                {
                    P2shotsuccess = 2;
                }
            }
            else if (monsterScriptMimon != null)
            {
                monsterScriptMimon.ShotByPlayer(shootingPlayer);
                if (isP1)
                {
                    P1shotsuccess = 3;
                }
                else
                {
                    P2shotsuccess = 3;
                }
            }
            else if (monsterScriptBaitmon != null)
            {
                monsterScriptBaitmon.ShotByPlayer(shootingPlayer);
                if (isP1)
                {
                    P1shotsuccess = 4;
                }
                else
                {
                    P2shotsuccess = 4;
                }
            }

            // Trigger vibration on hit with a 0.3s delay
            StartCoroutine(TriggerVibration(0.1f, 0.3f, 0.1f));
        }
    }
    else
    {
        Debug.Log("Raycast did not hit anything.");
    }

    Destroy(explosion, 0.6f);
}


    private IEnumerator TriggerVibration(float intensity, float delay, float duration)
    {
            // Stop any ongoing vibration
    if (isVibrating)
    {
        StopAllCoroutines();  // Stops any ongoing vibration
        ResetVibration();  // Ensure vibration is reset before starting a new one
    }

isVibrating = true;

        // Wait for the delay before starting vibration
        yield return new WaitForSeconds(delay);



        float startIntensity = intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float currentIntensity = Mathf.Lerp(startIntensity, 0, t * t); // Exponential decay

            if (assignedGamepad != null)
            {
                assignedGamepad.SetMotorSpeeds(currentIntensity, currentIntensity);
            }
            if (otherGamepad != null)
            {
                otherGamepad.SetMotorSpeeds(currentIntensity, currentIntensity);
            }

            elapsedTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ensure vibration stops completely
        if (assignedGamepad != null)
        {
            assignedGamepad.SetMotorSpeeds(0f, 0f);
        }
        if (otherGamepad != null)
        {
            otherGamepad.SetMotorSpeeds(0f, 0f);
        }

        isVibrating = false;
    }


public void ResetVibration()
{
    if (assignedGamepad != null)
    {
        assignedGamepad.SetMotorSpeeds(0f, 0f);
    }
    if (otherGamepad != null)
    {
        otherGamepad.SetMotorSpeeds(0f, 0f);
    }
    isVibrating = false;
}
}
