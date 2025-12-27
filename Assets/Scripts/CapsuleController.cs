using UnityEngine;

/// <summary>
/// Unity içinde kapsülün ve içindeki yumurtanın animasyonlarını yöneten kontrolcü.
/// </summary>
public class CapsuleController : MonoBehaviour
{
    [Header("Bileşen Bağlantıları")]
    [Tooltip("Kapsülün cam kısmını buraya sürükleyin.")]
    public Transform glassTransform;
    
    [Tooltip("Yumurtanın durduğu ışıklı platformun MeshRenderer'ını buraya sürükleyin.")]
    public MeshRenderer platformRenderer;

    [Header("Yumurta Ayarları")]
    [Tooltip("Kapsülün içine koyduğunuz yumurta objesini buraya sürükleyin.")]
    public Transform eggTransform;
    
    public float floatSpeed = 2f;      // Yukarı-aşağı süzülme hızı
    public float floatHeight = 0.15f;   // Süzülme mesafesi
    public float rotateSpeed = 30f;    // Kendi ekseninde dönme hızı

    [Header("Görsel Efektler")]
    public Color platformColor = new Color(0.3f, 0.8f, 1f); // Neon Mavi/Turkuaz
    public float lightIntensity = 2f;  // Işık şiddeti

    private Vector3 initialEggPos;

    void Start()
    {
        // Eğer bir yumurta atandıysa başlangıç pozisyonunu kaydet
        if (eggTransform != null)
        {
            initialEggPos = eggTransform.localPosition;
        }

        // Platform materyalinin rengini ve emisyonunu (ışık yayma) ayarla
        if (platformRenderer != null)
        {
            // Materyalin emisyon özelliğini açar
            platformRenderer.material.EnableKeyword("_EMISSION");
            
            // Hem ana rengi hem de ışık yayma rengini ayarla
            platformRenderer.material.SetColor("_Color", platformColor);
            platformRenderer.material.SetColor("_EmissionColor", platformColor * lightIntensity);
            
            // Sahne aydınlatmasını güncelle (Real-time GI için)
            DynamicGI.SetEmissive(platformRenderer, platformColor * lightIntensity);
        }
    }

    void Update()
    {
        // Yumurta Animasyonları
        if (eggTransform != null)
        {
            // 1. Süzülme Efekti (Sinüs dalgası kullanarak)
            float newY = initialEggPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            eggTransform.localPosition = new Vector3(eggTransform.localPosition.x, newY, eggTransform.localPosition.z);

            // 2. Kendi Ekseninde Dönme
            eggTransform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }

        // Cam Kapsül Efekti (İsteğe bağlı: Camın çok hafif dönmesini sağlayabiliriz)
        if (glassTransform != null)
        {
             glassTransform.Rotate(Vector3.up, (rotateSpeed * 0.2f) * Time.deltaTime);
        }
    }
}
