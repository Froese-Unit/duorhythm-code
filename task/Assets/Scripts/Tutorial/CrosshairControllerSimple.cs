using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairControllerSimple : MonoBehaviour
{
    public float speed = 5.0f;
    public GameObject explosionEffect; // Drag your explosion effect prefab here
    public float reloadTime = 0.7f;  // Time before player can shoot again
    public GameObject square;  // Drag the Square object here in the inspector
    public Image reloadBar;  // Drag the ReloadBar image here in the inspector

    private Vector2 boundary;
    private float halfWidth;
    private float halfHeight;
    private SpriteRenderer spriteRenderer;
    private float lastShotTime;  // Time the last shot was taken

    private static Gamepad gamepad;

    public static float playerLastShotTime; // Public static variable to track player's last shot time

    private void Start()
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

        // Initialize the gamepad variable
        gamepad = Gamepad.current;

        if (gamepad == null)
        {
            Debug.LogError("No gamepad connected.");
        }
    }

    void Update()
    {
        if (gamepad != null)
        {
            // Read the input from the active gamepad
            Vector2 moveInput = new Vector2(gamepad.leftStick.x.ReadValue(), gamepad.leftStick.y.ReadValue());
            // bool shootInput = gamepad.buttonSouth.isPressed; // Button South corresponds to "X" on DualSense
            bool shootInput = gamepad.rightShoulder.isPressed; // Button South corresponds to "X" on DualSense

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
                Shoot();
                lastShotTime = Time.time;
                playerLastShotTime = lastShotTime; // Update the player's last shot time


                if (reloadBar != null)
                {
                    reloadBar.fillAmount = 0f;
                    reloadBar.gameObject.SetActive(true);  // Show reload bar when shooting
                }
            }

            float clampedX = Mathf.Clamp(transform.position.x, -boundary.x + halfWidth, boundary.x - halfWidth);
            float clampedY = Mathf.Clamp(transform.position.y, -boundary.y + halfHeight, boundary.y - halfHeight);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }

    void Shoot()
    {
        Vector3 explosionPosition = new Vector3(transform.position.x, transform.position.y, 0);
        GameObject explosion = Instantiate(explosionEffect, explosionPosition, Quaternion.identity);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero);


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
            HandleMonsterShot(hit.collider, "Player1");
        }

        // Trigger vibration with a 0.3s delay
        StartCoroutine(TriggerVibration(0.1f, 0.3f, 0.1f));

        Destroy(explosion, 0.6f);
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

    private IEnumerator TriggerVibration(float intensity, float delay, float duration)
    {
        // Wait for the delay before starting vibration
        yield return new WaitForSeconds(delay);

        float startIntensity = intensity;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float currentIntensity = Mathf.Lerp(startIntensity, 0, t * t); // Exponential decay

            if (gamepad != null)
            {
                gamepad.SetMotorSpeeds(currentIntensity, currentIntensity);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure vibration stops completely
        if (gamepad != null)
        {
            gamepad.SetMotorSpeeds(0f, 0f);
        }
    }
}
