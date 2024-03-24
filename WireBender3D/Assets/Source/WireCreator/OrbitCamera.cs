using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 250.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float panSpeed = 0.5f;
    public float zoomSpeed = 5f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 8000f;
    public float distanceMin = .5f;
    public float distanceMax = 1500f;

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
       
        x = -100.0f; 
        y = 2.0f;

        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            // Orbiting
            if (Input.GetMouseButton(0)) // Left mouse button for orbiting
            {
                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);
            }

            // Panning
            if (Input.GetMouseButton(2)) // Middle mouse button (or two-finger drag) for panning
            {
                Vector3 moveDirection = (transform.right * -Input.GetAxis("Mouse X") + transform.up * -Input.GetAxis("Mouse Y")) * panSpeed;
                target.Translate(moveDirection, Space.World);
            }

            // Zooming
            distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
            distance = Mathf.Clamp(distance, distanceMin, distanceMax);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
