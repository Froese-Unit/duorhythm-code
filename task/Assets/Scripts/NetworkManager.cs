using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;
    private bool isRunning = false;
    private const string serverIp = "192.168.11.2";
    private const int serverPort = 12345;
    private MonsterSpawner monsterSpawner;
    private bool startSignalReceived = false;
    private bool isConnected = false;

    public bool isDebugMode = false; // This is the new debug mode flag

    void Start()
    {
        Debug.Log("NetworkManager: Start method called.");
        
        monsterSpawner = GameObject.FindObjectOfType<MonsterSpawner>();
        if (monsterSpawner == null)
        {
            Debug.LogError("NetworkManager: MonsterSpawner component not found!");
            return;
        }

        if (isDebugMode)
        {
            Debug.Log("NetworkManager: Debug mode is enabled, not connecting to server.");
            monsterSpawner.HideStartTextAndBeginTrial(); // Start the trial immediately in debug mode
        }
        else
        {
            Debug.Log("NetworkManager: Attempting to connect to server...");
            // Start the connection process asynchronously
            ConnectToServerAsync();
        }
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    private void ConnectToServerAsync()
    {
        clientThread = new Thread(() =>
        {
            Debug.Log("NetworkManager: Attempting to connect to server...");

            while (!isConnected)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(serverIp, serverPort);
                    stream = client.GetStream();
                    isConnected = true;
                    Debug.Log("NetworkManager: Connected to server.");
                }
                catch (Exception e)
                {
                    Debug.LogError("NetworkManager: Connection failed: " + e.Message);
                    Debug.Log("NetworkManager: Retrying in 5 seconds...");
                    Thread.Sleep(5000); // Wait for 5 seconds before retrying
                }
            }

            isRunning = true;
            WaitForStart();
        });
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void WaitForStart()
    {
        Debug.Log("NetworkManager: WaitForStart method called.");

        byte[] data = new byte[1024];
        int bytes;
        try
        {
            while (isRunning && !startSignalReceived)
            {
                if (stream != null && stream.DataAvailable && (bytes = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytes);
                    Debug.Log("NetworkManager: Message received: " + message);
                    if (message == "start")
                    {
                        Debug.Log("NetworkManager: Start signal received.");
                        startSignalReceived = true;
                        UnityMainThreadDispatcher.Instance().Enqueue(() => monsterSpawner.HideStartTextAndBeginTrial());
                    }
                }
                Thread.Sleep(100); // Prevent CPU spin
            }
        }
        catch (Exception e)
        {
            Debug.LogError("NetworkManager: Error while waiting for start signal: " + e.Message);
        }
    }

    public void SendTrialInfo(int blockNumber, int trialNumber, int totalTrials, int monsterID)
    {
        if (client != null && stream != null)
        {
            try
            {
                string trialInfo = $"Block {blockNumber}, Trial {trialNumber}/{totalTrials}, MonsterID: {monsterID}";
                byte[] trialInfoBytes = Encoding.UTF8.GetBytes(trialInfo);
                stream.Write(trialInfoBytes, 0, trialInfoBytes.Length);
                Debug.Log("NetworkManager: Trial info sent to server: " + trialInfo);
            }
            catch (Exception e)
            {
                Debug.LogError("NetworkManager: Error while sending trial info: " + e.Message);
            }
        }
    }

    public void SendDistanceInfo(float finalDistance)
    {
        if (client != null && stream != null)
        {
            try
            {
                string trialInfo = $"Distance: {finalDistance}";
                byte[] trialInfoBytes = Encoding.UTF8.GetBytes(trialInfo);
                stream.Write(trialInfoBytes, 0, trialInfoBytes.Length);
                Debug.Log("NetworkManager: Distance sent to server: " + trialInfo);
            }
            catch (Exception e)
            {
                Debug.LogError("NetworkManager: Error while sending distance: " + e.Message);
            }
        }
    }

    public void ExperimentEnded()
    {
        StartCoroutine(SendEndSignalAfterDelay(1)); // Adjust the delay if necessary
    }

    private IEnumerator SendEndSignalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SendEndSignalToServer();
    }

    private void SendEndSignalToServer()
    {
        Debug.Log("NetworkManager: Sending end signal to server...");

        if (client != null && stream != null)
        {
            try
            {
                byte[] endSignal = Encoding.UTF8.GetBytes("end");
                stream.Write(endSignal, 0, endSignal.Length);
                Debug.Log("NetworkManager: End signal sent to server.");
            }
            catch (Exception e)
            {
                Debug.LogError("NetworkManager: Error while sending end signal: " + e.Message);
            }
            finally
            {
                Cleanup();
            }
        }
        else
        {
            Debug.LogError("NetworkManager: Cannot send end signal, client or stream is null.");
        }
    }

    private void Cleanup()
    {
        Debug.Log("NetworkManager: Cleanup method called.");

        isRunning = false;

        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Abort(); // Forcefully terminate the thread
        }

        if (stream != null)
        {
            stream.Close();
        }

        if (client != null)
        {
            client.Close();
        }

        Debug.Log("NetworkManager: Cleaned up network resources.");
    }
}
