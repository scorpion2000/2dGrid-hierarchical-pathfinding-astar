using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public float panSpeed = 10f;

    void Update()
    {
        if (Input.GetMouseButton(1)) // right mouse button
        {
            var newPosition = new Vector3();
            newPosition.x = Input.GetAxis("Mouse X") * panSpeed;
            newPosition.y = Input.GetAxis("Mouse Y") * panSpeed;
            // translates to the opposite direction of mouse position.
            transform.Translate(-newPosition);
        }
    }

}
