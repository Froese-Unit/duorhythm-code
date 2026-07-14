using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    private float moveSpeed = 0f; // Speed at which the monster moves
    private Vector2 movementDirection;
    private Vector2 minBounds;
    private Vector2 maxBounds;
    private float steeringIntensity = 0f; // Intensity of the steering change

    private float noiseOffsetX;
    private float noiseOffsetY;

    private void Start()
    {
        // Set the initial movement direction to a random direction
        movementDirection = Random.insideUnitCircle.normalized;

        // Define the boundaries of the movement area
        minBounds = new Vector2(-5.5f, -2.0f); // Adjust these values based on your scene
        maxBounds = new Vector2(5.5f, 2.0f);

        // Initialize noise offsets with random values for each monster
        noiseOffsetX = Random.Range(0.0f, 100.0f);
        noiseOffsetY = Random.Range(0.0f, 100.0f);
    }

    private void Update()
    {
                // Check if moveSpeed is zero or less; if so, stop movement
        if (moveSpeed <= 0)
        {
            return; // Exit the Update method early
        }
        // Apply Perlin noise to create smooth directional changes over time
        float noiseX = Mathf.PerlinNoise(Time.time * steeringIntensity + noiseOffsetX, 0);
        float noiseY = Mathf.PerlinNoise(Time.time * steeringIntensity + noiseOffsetY, 0);

        Vector2 noiseDirection = new Vector2(noiseX - 0.5f, noiseY - 0.5f).normalized;
        movementDirection = (movementDirection + noiseDirection * Time.deltaTime).normalized;

        // Calculate new position
        Vector2 newPosition = (Vector2)transform.position + movementDirection * moveSpeed * Time.deltaTime;

        // Check if the monster is within the scene bounds
        if (newPosition.x < minBounds.x || newPosition.x > maxBounds.x)
        {
            movementDirection.x = -movementDirection.x; // Reverse direction on X-axis
            movementDirection = AdjustDirection(movementDirection, isHorizontal: true); // Adjust direction
        }
        if (newPosition.y < minBounds.y || newPosition.y > maxBounds.y)
        {
            movementDirection.y = -movementDirection.y; // Reverse direction on Y-axis
            movementDirection = AdjustDirection(movementDirection, isHorizontal: false); // Adjust direction
        }

        // Update position
        transform.position = (Vector3)newPosition;
    }

    private Vector2 AdjustDirection(Vector2 direction, bool isHorizontal)
    {
        // Adjust the direction slightly to avoid purely horizontal or vertical movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Adjust angle conditionally to ensure it doesn't lead to out-of-bounds movement
        if (isHorizontal)
        {
            if (Mathf.Abs(direction.y) < 0.1f) // Avoid pure horizontal
            {
                direction.y += Random.Range(0.1f, 0.3f); // Slightly adjust the Y component
            }
        }
        else
        {
            if (Mathf.Abs(direction.x) < 0.1f) // Avoid pure vertical
            {
                direction.x += Random.Range(0.1f, 0.3f); // Slightly adjust the X component
            }
        }

        // Normalize direction to keep the speed consistent
        direction.Normalize();

        return direction;
    }
        // Public method to set moveSpeed
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // Public method to set steeringIntensity
    public void SetSteeringIntensity(float intensity)
    {
        steeringIntensity = intensity;
    }
}
