using UnityEngine;
using LSL;
using System.IO.Ports;

// Reads "PULSE" lines from an Arduino over serial and pushes each one as an
// LSL marker, so the offset between the Arduino's BNC trigger (seen by the
// EEG amp) and this timestamp (seen by Unity) can be measured for sync testing.
public class ArduinoPulseMarker : MonoBehaviour
{
    [SerializeField] private string portName = ""; // Set Serial Port Identifier here for the Arduino connection, eg. COM3
    [SerializeField] private int baudRate = 9600;

    private SerialPort serial;
    private StreamOutlet outlet;

    void Start()
    {
        var streamInfo = new StreamInfo("ArduinoPulseMarkers", "Markers", 1, 0.0,
            channel_format_t.cf_string, "arduino_pulse_" + SystemInfo.deviceUniqueIdentifier);
        outlet = new StreamOutlet(streamInfo);

        try
        {
            serial = new SerialPort(portName, baudRate) { ReadTimeout = 50 };
            serial.Open();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"ArduinoPulseMarker: could not open {portName} ({e.Message}). Marker stream will stay idle.");
        }
    }

    void Update()
    {
        if (serial == null || !serial.IsOpen) return;

        try
        {
            while (serial.BytesToRead > 0)
            {
                string msg = serial.ReadLine();
                if (msg.Contains("PULSE"))
                {
                    outlet.push_sample(new string[] { "arduino_pulse" });
                    Debug.Log($"ArduinoPulseMarker: pulse received at {Time.realtimeSinceStartup:F3}s");
                }
            }
        }
        catch (System.TimeoutException) { }
    }

    void OnApplicationQuit()
    {
        if (serial != null && serial.IsOpen) serial.Close();
    }
}
