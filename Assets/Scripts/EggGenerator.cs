using UnityEngine;

using UnityEngine;

[ExecuteAlways] // Runs in Editor and Play Mode
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EggGenerator : MonoBehaviour
{
    [Header("Settings")]
    public int resolution = 20;
    public float size = 1f;
    public Color eggColor = Color.white;

    void Start()
    {
        Generate();
    }

    void OnValidate()
    {
        // Regenerate when values change in Inspector
        Generate();
    }

    void Generate()
    {
        GenerateEggMesh();
        UpdateMaterial();
    }

    void UpdateMaterial()
    {
        // Check if we already have a functional material to avoid resetting it constantly
        Renderer r = GetComponent<Renderer>();
        if (r.sharedMaterial == null || r.sharedMaterial.color != eggColor)
        {
             // Add shader logic similar to CatGenerator
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            
            Material m = new Material(s);
            m.color = eggColor;
            if (s.name.Contains("Universal"))
                m.SetColor("_BaseColor", eggColor);
                
            r.sharedMaterial = m; // Use sharedMaterial in Editor
        }
    }

    void GenerateEggMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf.sharedMesh != null && mf.sharedMesh.name == "ProceduralEgg")
        {
            // Keep existing mesh instance if possible to avoid memory leak in Editor
        }
        else
        {
            mf.mesh = new Mesh();
            mf.sharedMesh.name = "ProceduralEgg";
        }
        
        Mesh mesh = mf.sharedMesh;
        mesh.Clear();

        // Mathematical Sphere Generation (UV Sphere)
        // No need to create/destroy GameObjects!
        
        int lat = resolution; // latitude segments
        int lon = resolution; // longitude segments
        float radius = size * 0.5f;

        Vector3[] vertices = new Vector3[(lon + 1) * lat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        for (int latI = 0; latI < lat; latI++)
        {
            float a1 = _pi * (float)(latI + 1) / (lat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lonI = 0; lonI <= lon; lonI++)
            {
                float a2 = _2pi * (float)(lonI == lon ? 0 : lonI) / lon;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                Vector3 v = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
                
                // DEFORM TO EGG HERE
                float taper = (v.y > 0) ? (1.0f - v.y * 0.2f) : 1.0f;
                float stretch = (v.y > 0) ? 1.2f : 0.9f;
                
                vertices[lonI + latI * (lon + 1) + 1] = new Vector3(v.x * taper, v.y * stretch, v.z * taper);
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius * 0.9f; // Bottom stretches less

        // Normal/UVs are complex to do manually perfect, but Recalculate will handle Normals.
        // Let's rely on RecalculateNormals() for lighting.
        
        // Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        int i = 0;
        // Top Cap
        for (int lonI = 0; lonI < lon; lonI++)
        {
            triangles[i++] = lonI + 2;
            triangles[i++] = lonI + 1;
            triangles[i++] = 0;
        }

        // Middle
        for (int latI = 0; latI < lat - 1; latI++)
        {
            for (int lonI = 0; lonI < lon; lonI++)
            {
                int current = lonI + latI * (lon + 1) + 1;
                int next = current + lon + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        // Bottom Cap
        for (int lonI = 0; lonI < lon; lonI++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) + lonI + 1;
            triangles[i++] = vertices.Length - (lon + 2) + lonI;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
