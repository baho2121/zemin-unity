using UnityEngine;

[ExecuteAlways]
public class EggMachineGenerator : MonoBehaviour
{
    [Header("Settings")]
    public bool generate = false;
    public Color machineColor = new Color(0.3f, 0.8f, 1f);

    void Update()
    {
        if (generate)
        {
            generate = false;
            BuildMachine();
        }
    }

    void BuildMachine()
    {
        // 1. Root Setup
        name = "EggMachine";
        CapsuleController controller = GetComponent<CapsuleController>();
        if (controller == null) controller = gameObject.AddComponent<CapsuleController>();

        // INTERACTION SETUP
        // 1. EggBase Script (Kept for Data Holding if needed, but logic stripped)
        EggBase eggBase = GetComponent<EggBase>();
        if (eggBase == null) eggBase = gameObject.AddComponent<EggBase>();
        
        // Removed Trigger Collider and UI as requested.


        // COLORS
        Color darkColor = new Color(0.15f, 0.15f, 0.15f); // Dark Grey/Black
        Color lightBlue = new Color(0.3f, 0.8f, 1f); // Neon Blue

        // 2. BASE STRUCTURE
        // A. Base Ring (Dark Bottom)
        Transform baseTr = transform.Find("BaseRing");
        if (baseTr == null)
        {
            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            b.name = "BaseRing";
            b.transform.SetParent(transform, false);
            b.transform.localScale = new Vector3(2.2f, 0.15f, 2.2f);
            b.transform.localPosition = Vector3.up * 0.075f;
            SetColor(b, darkColor);
            baseTr = b.transform;
            DestroyImmediate(b.GetComponent<Collider>()); // Use root collider
        }

        // B. Inner Pad (Neon Light)
        Transform padTr = transform.Find("InnerPad");
        if (padTr == null)
        {
            GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            p.name = "InnerPad";
            p.transform.SetParent(transform, false);
            p.transform.localScale = new Vector3(1.7f, 0.2f, 1.7f); 
            p.transform.localPosition = Vector3.up * 0.15f; 
            padTr = p.transform;
            DestroyImmediate(p.GetComponent<Collider>());
        }
        controller.platformRenderer = padTr.GetComponent<MeshRenderer>();
        controller.platformColor = lightBlue; 

        // 3. GLASS DOME (Capsule)
        Transform glassTr = transform.Find("GlassDome");
        if (glassTr == null)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Capsule); 
            g.name = "GlassDome";
            g.transform.SetParent(transform, false);
            g.transform.localPosition = Vector3.up * 1.0f; 
            g.transform.localScale = new Vector3(1.6f, 1.0f, 1.6f); 
            glassTr = g.transform;
            SetupGlassMaterial(g);
        }
        controller.glassTransform = glassTr;

        // 4. TOP LID (Dark Cap)
        Transform lidTr = transform.Find("TopLid");
        if (lidTr == null)
        {
            GameObject l = GameObject.CreatePrimitive(PrimitiveType.Sphere); 
            l.name = "TopLid";
            l.transform.SetParent(glassTr, false); 
            l.transform.ParentTo(transform); // Actually parent to root to avoid scale issues?
            // Re-parent correctly:
            l.transform.SetParent(transform, false);
            l.transform.localPosition = Vector3.up * 1.9f; 
            l.transform.localScale = new Vector3(1.65f, 0.4f, 1.65f); 
            SetColor(l, darkColor);
            lidTr = l.transform;
            DestroyImmediate(l.GetComponent<Collider>());
        }

        // 5. Egg Slot Hint
        Debug.Log("Egg Machine Updated to Capsule Design!");

        // UI Prompt Removed as requested.
        // CLEANUP: Destroy specific old UI artifacts if they exist
        Transform oldUI = transform.Find("InteractionPrompt");
        if (oldUI != null)
        {
            DestroyImmediate(oldUI.gameObject);
        }
        
        // Assign to EggBase
        eggBase.interactionUI = null; // No UI
    }

    void SetColor(GameObject obj, Color c)
    {
        Renderer r = obj.GetComponent<Renderer>();
        Material m = new Material(Shader.Find("Standard"));
        m.color = c;
        if (Shader.Find("Universal Render Pipeline/Lit") != null)
        {
            m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.SetColor("_BaseColor", c);
        }
        r.sharedMaterial = m;
    }

    void SetupGlassMaterial(GameObject g)
    {
        Renderer r = g.GetComponent<Renderer>();
        
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        
        Material mat = new Material(shader);
        
        if (shader.name.Contains("Universal"))
        {
            mat.SetFloat("_Surface", 1.0f); // Transparent
            mat.SetFloat("_Blend", 0.0f);   // Alpha
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.SetShaderPassEnabled("ShadowCaster", false); 
            mat.renderQueue = 3000;
            mat.SetColor("_BaseColor", new Color(0.6f, 0.8f, 1f, 0.1f)); 
            mat.SetFloat("_Smoothness", 0.8f); 
        }
        else
        {
            mat.color = new Color(0.8f, 0.95f, 1f, 0.15f);
            mat.SetFloat("_Mode", 3); 
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
        r.sharedMaterial = mat;
    }
}

public static class TransformExtensions
{
    public static void ParentTo(this Transform t, Transform parent)
    {
        t.SetParent(parent, false);
    }
}
