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
            if (player == null) player = GameObject.Find("Character"); // Fallback name
            if (player != null) target = player.transform;
        }

        // Setup initial angles based on current rotation
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // Lock cursor for TPS feel? 
        // NO: We need cursor for clicking coins. 
        // Change: Only lock when holding Right Click (Roblox style)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Camera Rotation (Only when Right Click is held)
        if (Input.GetMouseButton(1))
        {
            // Lock cursor while dragging
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            currentX += Input.GetAxis("Mouse X") * sensitivityX;
            currentY -= Input.GetAxis("Mouse Y") * sensitivityY;
            currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            // Unlock cursor when not dragging
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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
