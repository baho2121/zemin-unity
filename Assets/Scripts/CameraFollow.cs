using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float sensitivityX = 4.0f;
    public float sensitivityY = 1.0f;
    public float minVerticalAngle = -20.0f;
    public float maxVerticalAngle = 80.0f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 2.0f;
    public float minDistance = 2.0f;
    public float maxDistance = 10.0f;

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    void Start()
    {
        // Auto-find target if not set
        if (target == null)
        {
            GameObject player = GameObject.Find("Capsule");
            if (player != null) target = player.transform;
        }

        // Setup initial angles based on current rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Lock cursor for TPS feel
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse Input
        currentX += Input.GetAxis("Mouse X") * sensitivityX;
        currentY -= Input.GetAxis("Mouse Y") * sensitivityY;

        currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);

        // Zoom Input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculate rotation and position
        Vector3 dir = new Vector3(0, 0, -distance);
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        
        // Look specific logic
        Vector3 focusPoint = target.position + Vector3.up * 1.5f; 
        transform.position = focusPoint + rotation * dir;
        transform.LookAt(focusPoint);
    }
}
