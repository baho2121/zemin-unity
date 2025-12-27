using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public LayerMask interactableLayer; // Create a Layer for "Breakables" later

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left Click
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                // Debug: What did we hit?
                Debug.Log("Raycast Hit: " + hit.collider.name + " (Tag: " + hit.collider.tag + ")");

                // Check if we hit a BreakableObject directly
                BreakableObject breakable = hit.collider.GetComponent<BreakableObject>();
                
                // Or maybe it's in parent?
                if (breakable == null)
                    breakable = hit.collider.GetComponentInParent<BreakableObject>();

                if (breakable != null)
                {
                    Debug.Log("Found Breakable: " + breakable.name);
                    // Command Pets to Attack!
                    if (PetManager.Instance != null)
                    {
                        PetManager.Instance.AttackTarget(breakable);
                    }
                    else
                    {
                        Debug.LogWarning("No PetManager found in scene!");
                    }
                }
            }
        }
    }
}
