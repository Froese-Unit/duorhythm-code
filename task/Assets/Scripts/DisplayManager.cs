using UnityEngine;

public class DisplayManager : MonoBehaviour
{
    public Camera CameraP1;  // Camera for Player 1
    public Camera CameraP2;  // Camera for Player 2

    void Start()
    {
        // Check if more than one display is available
        if (Display.displays.Length > 1)
        {
            Display.displays[0].Activate();
            Display.displays[1].Activate();
            
            CameraP1.targetDisplay = 0;  // Set CameraP1 to render on Monitor 1
            CameraP2.targetDisplay = 1;  // Set CameraP2 to render on Monitor 2
        }
    }
}
