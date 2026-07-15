using UnityEngine;
using LSL;
using System.Diagnostics;

public class LSLSender : MonoBehaviour
{
    private const string StreamName = "Task";
    private const string StreamType = "GameData";
    private const float SamplingRate = 100f;  // 100 Hz

    private const int ChannelCount = 14; // 6 for crosshair data (3 per player: x,y,velocity), 2 for shooting, 2 for hits, 1 for trials/monster spawns, 1 for score

    private StreamOutlet outlet;
    private float[] sample = new float[ChannelCount];

    private GameObject CrosshairP1;
    private GameObject CrosshairP2;
    private Vector3 prevPositionP1, prevPositionP2;
    private float prevTimeP1;
    private float prevTimeP2;

    private Stopwatch stopwatch;

    void Start()
    {
        Time.fixedDeltaTime = 1f / SamplingRate;  // Set FixedUpdate interval to match the sampling rate

        var hash = new Hash128();
        hash.Append(StreamName);
        hash.Append(StreamType);
        hash.Append(gameObject.GetInstanceID());

        StreamInfo streamInfo = new StreamInfo(StreamName, StreamType, ChannelCount, SamplingRate, channel_format_t.cf_float32, hash.ToString());
        outlet = new StreamOutlet(streamInfo);

        CrosshairP1 = GameObject.Find("CrosshairP1");
        CrosshairP2 = GameObject.Find("CrosshairP2");

        if (CrosshairP1 == null || CrosshairP2 == null)
        {
            UnityEngine.Debug.LogError("Crosshair objects not found!");
        }

        prevPositionP1 = CrosshairP1.transform.position;
        prevPositionP2 = CrosshairP2.transform.position;
        prevTimeP1 = Time.time;
        prevTimeP2 = Time.time;

        stopwatch = new Stopwatch();
        stopwatch.Start();
    }

    void FixedUpdate()
    {
            // Check and assign CrosshairP1 if it's null
    if (CrosshairP1 == null)
    {
        CrosshairP1 = GameObject.Find("CrosshairP1");
    }

    // Check and assign CrosshairP2 if it's null
    if (CrosshairP2 == null)
    {
        CrosshairP2 = GameObject.Find("CrosshairP2");
    }


        // Fetching Player 1 Data
        Vector2 posP1 = GetCrosshairPositionPlayer1();
        Vector2 velP1 = GetCrosshairVelocityPlayer1();

        // Fetching Player 2 Data
        Vector2 posP2 = GetCrosshairPositionPlayer2();
        Vector2 velP2 = GetCrosshairVelocityPlayer2();

        // Player 1's Data
        sample[0] = posP1.x;
        sample[1] = posP1.y;
        sample[2] = velP1.x;
        sample[3] = velP1.y;
        sample[4] = GetShootingPressesPlayer1();
        sample[5] = GetTargetHitConfirmsPlayer1();

        // Player 2's Data
        sample[6] = posP2.x;
        sample[7] = posP2.y;
        sample[8] = velP2.x;
        sample[9] = velP2.y;
        sample[10] = GetShootingPressesPlayer2();
        sample[11] = GetTargetHitConfirmsPlayer2();

        // Other Data
        sample[12] = GetTrialOrMonsterSpawn();
        sample[13] = GetScore();
        
        // UnityEngine.Debug.Log("Sending data: " + string.Join(",", sample));
        outlet.push_sample(sample);

        // Reset shooting flags
        CrosshairController.hasP1Shot = false;
        CrosshairController.hasP2Shot = false;
        CrosshairController.P1shotsuccess = 0;
        CrosshairController.P2shotsuccess = 0;
        MonsterSpawner.monsterSpawnState = 0;
    }

    // Player 1's methods
    Vector2 GetCrosshairPositionPlayer1()
    {
        if (CrosshairP1 != null)
        {
            return new Vector2(CrosshairP1.transform.position.x, CrosshairP1.transform.position.y);
        }
        return Vector2.zero;
    }

    Vector2 GetCrosshairVelocityPlayer1()
    {
        if (CrosshairP1 != null)
        {
            float currentTime = Time.time;
            float deltaTime = currentTime - prevTimeP1;

            if (deltaTime == 0) // Avoid division by zero
                return Vector2.zero;

            Vector3 currentVelocity = (CrosshairP1.transform.position - prevPositionP1) / deltaTime;
            prevPositionP1 = CrosshairP1.transform.position;
            prevTimeP1 = currentTime;
            return new Vector2(currentVelocity.x, currentVelocity.y);
        }
        return Vector2.zero;
    }

    float GetShootingPressesPlayer1()
    {
        return CrosshairController.hasP1Shot ? 1f : 0f;
    }

    float GetTargetHitConfirmsPlayer1()
    {
        return CrosshairController.P1shotsuccess; 
    }

    // Player 2's methods
    Vector2 GetCrosshairPositionPlayer2()
    {
        if (CrosshairP2 != null)
        {
            return new Vector2(CrosshairP2.transform.position.x, CrosshairP2.transform.position.y);
        }
        return Vector2.zero;
    }

    Vector2 GetCrosshairVelocityPlayer2()
    {
        if (CrosshairP2 != null)
        {
            float currentTime = Time.time;
            float deltaTime = currentTime - prevTimeP2;

            if (deltaTime == 0) // Avoid division by zero
                return Vector2.zero;

            Vector3 currentVelocity = (CrosshairP2.transform.position - prevPositionP2) / deltaTime;
            prevPositionP2 = CrosshairP2.transform.position;
            prevTimeP2 = currentTime;
            return new Vector2(currentVelocity.x, currentVelocity.y);
        }
        return Vector2.zero;
    }

    float GetShootingPressesPlayer2()
    {
        return CrosshairController.hasP2Shot ? 1f : 0f;
    }

    float GetTargetHitConfirmsPlayer2()
    {
        return CrosshairController.P2shotsuccess; 
    }

    // Other methods
    float GetScore()
    {
        return ScoreManager.instance.score; // Placeholder
    }

    float GetTrialOrMonsterSpawn()
    {
        return MonsterSpawner.monsterSpawnState;
    }
}

