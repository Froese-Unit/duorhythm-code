using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    private void Update()
    {
        if (target)
        {
            // Use the camera's current x and y positions
            float cameraXPosition = transform.position.x;
            float cameraYPosition = transform.position.y;
            float cameraZPosition = target.position.z + offset.z;

            transform.position = new Vector3(cameraXPosition, cameraYPosition, cameraZPosition);
        }
    }



}
