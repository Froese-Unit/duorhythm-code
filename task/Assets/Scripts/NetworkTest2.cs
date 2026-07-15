using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class NetworkTest2 : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;
    private bool isRunning = false;
    private const string serverIp = "192.168.11.2"; // Server IP address
    private const int serverPort = 12345; // Server port

    void Start()
    {
        Debug.Log("NetworkTest2: Start method called.");
        ConnectToServerAsync();
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    private void ConnectToServerAsync()
    {
        clientThread = new Thread(() =>
        {
            Debug.Log("NetworkTest2: Attempting to connect to server...");

            while (!isRunning)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(serverIp, serverPort);
                    stream = client.GetStream();
                    isRunning = true;
                    Debug.Log("NetworkTest2: Connected to server.");

                    // Wait for start signal
                    WaitForStart();
                }
                catch (Exception e)
                {
                    Debug.LogError("NetworkTest2: Connection failed: " + e.Message);
                    Debug.Log("NetworkTest2: Retrying in 5 seconds...");
                    Thread.Sleep(5000); // Wait for 5 seconds before retrying
                }
            }
        });
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void WaitForStart()
    {
        Debug.Log("NetworkTest2: WaitForStart method called.");

        byte[] data = new byte[1024];
        int bytes;
        try
        {
            while (isRunning)
            {
                if (stream != null && stream.DataAvailable && (bytes = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytes);
                    Debug.Log("NetworkTest2: Message received: " + message);
                    if (message == "start")
                    {
                        Debug.Log("NetworkTest2: Start signal received.");
                        // Simulate sending trial information
                        SendTrialInfo("Block 1, Trial 1/5, MonsterID: 123");
                        Thread.Sleep(2000); // Simulate some delay between trials
                        SendTrialInfo("Block 1, Trial 2/5, MonsterID: 124");
                        Thread.Sleep(2000); // Simulate some delay between trials
                        SendTrialInfo("Block 1, Trial 3/5, MonsterID: 125");
                        Thread.Sleep(2000); // Simulate some delay between trials
                        SendEndSignal();
                        break;
                    }
                }
                Thread.Sleep(100); // Prevent CPU spin
            }
        }
        catch (Exception e)
        {
            Debug.LogError("NetworkTest2: Error while waiting for start signal: " + e.Message);
        }
    }

    private void SendTrialInfo(string trialInfo)
    {
        if (client != null && stream != null)
        {
            try
            {
                byte[] trialInfoBytes = Encoding.UTF8.GetBytes(trialInfo);
                stream.Write(trialInfoBytes, 0, trialInfoBytes.Length);
                Debug.Log("NetworkTest2: Trial info sent to server: " + trialInfo);
            }
            catch (Exception e)
            {
                Debug.LogError("NetworkTest2: Error while sending trial info: " + e.Message);
            }
        }
    }

    private void SendEndSignal()
    {
        Debug.Log("NetworkTest2: Sending end signal to server...");

        if (client != null && stream != null)
        {
            try
            {
                byte[] endSignal = Encoding.UTF8.GetBytes("end");
                stream.Write(endSignal, 0, endSignal.Length);
                Debug.Log("NetworkTest2: End signal sent to server.");
            }
            catch (Exception e)
            {
                Debug.LogError("NetworkTest2: Error while sending end signal: " + e.Message);
            }
            finally
            {
                Cleanup();
            }
        }
    }

    private void Cleanup()
    {
        Debug.Log("NetworkTest2: Cleanup method called.");

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

        Debug.Log("NetworkTest2: Cleaned up network resources.");
    }
}
