using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private float moveSpeed = 2f;
    private float fadeSpeed = 3f;
    private TextMesh textMesh;
    private Color textColor;
    private Transform mainCameraTransform;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        if (textMesh == null)
            textMesh = gameObject.AddComponent<TextMesh>();
            
        textColor = textMesh.color;
        
        // Optimize: Cache camera
        if (Camera.main != null)
            mainCameraTransform = Camera.main.transform;
    }

    public void Setup(float damageAmount)
    {
        textMesh.text = damageAmount.ToString("0");
        textMesh.characterSize = 0.2f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.fontSize = 24;
        textMesh.color = Color.white; // Default color
    }
    
    // "Privacy" setting: Only visible to owner?
    // Since this is currently local, it's visible to us.
    // To implement "Only visible to owner" in multiplayer, you'd only Instantiate this for the localClient.

    void Update()
    {
        // 1. Float Up
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // 2. Face Camera (Billboard)
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                             mainCameraTransform.rotation * Vector3.up);
        }

        // 3. Fade Out
        textColor.a -= fadeSpeed * Time.deltaTime;
        textMesh.color = textColor;

        if (textColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
