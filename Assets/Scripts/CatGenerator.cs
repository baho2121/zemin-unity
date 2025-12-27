using UnityEngine;

public class CatGenerator : MonoBehaviour
{
    [Header("Generation")]
    public bool generateOnStart = true;

    [Header("Materials (Auto-Generated if Null)")]
    public Material mainMat;    // Orange
    public Material detailMat;  // Dark Orange
    public Material darkMat;    // Black/Dark Grey
    public Material pinkMat;    // Pink
    public Material whiteMat;   // White
    public Material mouthMat;   // Light Orange/Peach

    [Header("Adjustments")]
    public float globalScale = 0.3f; // Make it smaller by default

    private GameObject catGroup;
    private GameObject tailObject; 

    void Start()
    {
        if (generateOnStart)
        {
            GenerateCat();
        }
    }

    public void GenerateCat()
    {
        // Cleanup old
        if (catGroup != null) Destroy(catGroup);
        catGroup = new GameObject("ProceduralCat");
        catGroup.transform.SetParent(transform, false);
        catGroup.transform.localScale = Vector3.one * globalScale; // Apply global scale

        // 0. Setup Materials (if missing)
        SetupMaterials();

        // 1. BODY
        // ThreeJS: BoxGeometry(1.1, 1, 1)
        CreatePart("Body", new Vector3(1.1f, 1f, 1f), Vector3.zero, Vector3.zero, mainMat);

        // 2. EARS
        // ThreeJS: BoxGeometry(0.3, 0.4, 0.15)
        // Left Ear: pos(-0.35, 0.6, 0.1), rot.z = 0.2
        CreatePart("LeftEar", new Vector3(0.3f, 0.4f, 0.15f), new Vector3(-0.35f, 0.6f, 0.1f), new Vector3(0, 0, -11.5f), detailMat); 
        // Right Ear: pos(0.35, 0.6, 0.1), rot.z = -0.2
        CreatePart("RightEar", new Vector3(0.3f, 0.4f, 0.15f), new Vector3(0.35f, 0.6f, 0.1f), new Vector3(0, 0, 11.5f), detailMat);

        // 3. EYES
        // Left Eye
        CreatePart("LeftEyeWhite", new Vector3(0.22f, 0.22f, 0.05f), new Vector3(-0.28f, 0.2f, 0.5f), Vector3.zero, whiteMat);
        CreatePart("LeftPupil", new Vector3(0.1f, 0.15f, 0.06f), new Vector3(-0.28f, 0.2f, 0.51f), Vector3.zero, darkMat);

        // Right Eye
        CreatePart("RightEyeWhite", new Vector3(0.22f, 0.22f, 0.05f), new Vector3(0.28f, 0.2f, 0.5f), Vector3.zero, whiteMat);
        CreatePart("RightPupil", new Vector3(0.1f, 0.15f, 0.06f), new Vector3(0.28f, 0.2f, 0.51f), Vector3.zero, darkMat);

        // 4. MOUTH & NOSE
        CreatePart("Snout", new Vector3(0.4f, 0.25f, 0.1f), new Vector3(0, -0.05f, 0.51f), Vector3.zero, mouthMat);
        CreatePart("Nose", new Vector3(0.12f, 0.08f, 0.05f), new Vector3(0, 0.02f, 0.56f), Vector3.zero, pinkMat);

        // 5. PAWS
        Vector3 pawScale = new Vector3(0.25f, 0.15f, 0.25f);
        CreatePart("PawFL", pawScale, new Vector3(-0.35f, -0.55f, 0.3f), Vector3.zero, mainMat);
        CreatePart("PawFR", pawScale, new Vector3(0.35f, -0.55f, 0.3f), Vector3.zero, mainMat);
        CreatePart("PawBL", pawScale, new Vector3(-0.35f, -0.55f, -0.3f), Vector3.zero, mainMat);
        CreatePart("PawBR", pawScale, new Vector3(0.35f, -0.55f, -0.3f), Vector3.zero, mainMat);

        // 6. TAIL
        tailObject = CreatePart("Tail", new Vector3(0.15f, 0.15f, 0.5f), new Vector3(0, -0.2f, -0.6f), new Vector3(17f, 0, 0), detailMat);
    }

    private GameObject CreatePart(string name, Vector3 scale, Vector3 localPos, Vector3 rotation, Material mat)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(catGroup.transform);
        part.transform.localPosition = localPos;
        part.transform.localEulerAngles = rotation;
        // Apply global scale is done on parent, so localScale matches 3js logic
        part.transform.localScale = scale;

        // Visuals
        Renderer rend = part.GetComponent<Renderer>();
        rend.material = mat;
        
        Destroy(part.GetComponent<Collider>()); 

        return part;
    }

    void SetupMaterials()
    {
        // Try to find a valid shader. 
        // 1. URP Lit (High Quality)
        Shader s = Shader.Find("Universal Render Pipeline/Lit");
        // 2. URP Simple
        if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
        // 3. Fallback Standard
        if (s == null) s = Shader.Find("Standard");
        // 4. Fallback Mobile
        if (s == null) s = Shader.Find("Mobile/Diffuse");
        
        if (mainMat == null) mainMat = CreateMat(new Color(1f, 0.647f, 0f), s); 
        if (detailMat == null) detailMat = CreateMat(new Color(0.9f, 0.49f, 0.13f), s); 
        if (darkMat == null) darkMat = CreateMat(new Color(0.13f, 0.13f, 0.13f), s); 
        if (pinkMat == null) pinkMat = CreateMat(new Color(1f, 0.71f, 0.76f), s); 
        if (whiteMat == null) whiteMat = CreateMat(Color.white, s);
        if (mouthMat == null) mouthMat = CreateMat(new Color(1f, 0.85f, 0.73f), s); 
    }

    Material CreateMat(Color c, Shader shader)
    {
        if (shader == null)
        {
            Debug.LogError("CatGenerator: Could not find any valid shader! Materials will be pink.");
            return new Material(Shader.Find("Standard")); // Last ditch effort
        }

        Material m = new Material(shader);
        m.color = c;
        
        // URP often requires setting "_BaseColor" instead of main color
        if (shader.name.Contains("Universal"))
        {
            m.SetColor("_BaseColor", c);
        }
        
        return m;
    }
    
    // Animation Logic (Replicating the JS Example)
    void Update()
    {
        if (catGroup == null || tailObject == null) return;
        
        float time = Time.time * 1000f; // JS Date.now() is ms

        // Kuyruk Sallama (Tail Wag)
        // cat.tail.rotation.z = Math.sin(time * 0.005) * 0.2;
        // In Unity Z is also forward/roll depending on axes. Let's try Z (Roll) or Y (Yaw).
        // Standard primitives: Z is depth.
        float tailShake = Mathf.Sin(time * 0.005f) * 0.2f * Mathf.Rad2Deg; // Convert to degrees
        
        // Need to add to base rotation (17, 0, 0)
        // The tail primitive default alignment might be different from ThreeJS box.
        // Assuming Unity Cube: Logic matches.
        Vector3 currentRot = tailObject.transform.localEulerAngles;
        tailObject.transform.localEulerAngles = new Vector3(17f, tailShake, 0f); // Rotate around Y axis for wagging left-right

        // Zıplama Efekti (Bounce)
        // cat.model.position.y = Math.abs(Math.sin(time * 0.002)) * 0.5;
        float bounce = Mathf.Abs(Mathf.Sin(time * 0.002f)) * 0.5f;
        catGroup.transform.localPosition = new Vector3(0, bounce, 0);
    }
}
