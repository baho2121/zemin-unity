using UnityEngine;

public class EggAnimation : MonoBehaviour {
    public float floatSpeed = 1.5f;
    public float floatAmount = 0.1f;
    public float rotateSpeed = 20f;
    
    private Vector3 initialPos;

    void Start()
    {
        initialPos = transform.localPosition;
    }

    void Update() {
        // Süzülme (Floating)
        float newY = initialPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = new Vector3(initialPos.x, newY, initialPos.z);
        
        // Kendi ekseninde dönme (Rotating)
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
