using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SimpleClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;
    private bool isRunning = false;
    private const string serverIp = "192.168.11.2"; // Server IP address
    private const int serverPort = 12345; // Server port

    void Start()
    {
        Debug.Log("SimpleClient: Start method called.");
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
            Debug.Log("SimpleClient: Attempting to connect to server...");

            while (!isRunning)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(serverIp, serverPort);
                    stream = client.GetStream();
                    isRunning = true;
                    Debug.Log("SimpleClient: Connected to server.");

                    // Start receiving data
                    ReceiveData();
                }
                catch (Exception e)
                {
                    Debug.LogError("SimpleClient: Connection failed: " + e.Message);
                    Debug.Log("SimpleClient: Retrying in 5 seconds...");
                    Thread.Sleep(5000); // Wait for 5 seconds before retrying
                }
            }
        });
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            byte[] data = new byte[1024];
            int bytes;
            while (isRunning)
            {
                if (stream.DataAvailable && (bytes = stream.Read(data, 0, data.Length)) != 0)
                {
                    string message = Encoding.UTF8.GetString(data, 0, bytes);
                    Debug.Log("SimpleClient: Message received: " + message);
                    if (message == "start")
                    {
                        Debug.Log("SimpleClient: Start signal received.");
                        // Handle the start signal as needed
                    }
                }
                Thread.Sleep(100); // Prevent CPU spin
            }
        }
        catch (Exception e)
        {
            Debug.LogError("SimpleClient: Error while receiving data: " + e.Message);
        }
    }

    private void Cleanup()
    {
        Debug.Log("SimpleClient: Cleanup method called.");

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

        Debug.Log("SimpleClient: Cleaned up network resources.");
    }
}
