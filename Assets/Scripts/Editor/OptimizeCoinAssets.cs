using UnityEngine;
using UnityEditor;

public class OptimizeCoinAssets
{
    [MenuItem("Tools/Optimize Coin Assets")]
    public static void Optimize()
    {
        string folderPath = "Assets/Coin";
        // Find all assets in the folder
        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AssetImporter importer = AssetImporter.GetAtPath(path);

            // Optimize Models (FBX, OBJ, etc.)
            if (importer is ModelImporter modelImporter)
            {
                modelImporter.meshCompression = ModelImporterMeshCompression.High;
                modelImporter.importCameras = false; // Don't import unnecessary cameras
                modelImporter.importLights = false;  // Don't import unnecessary lights
                modelImporter.isReadable = false;    // Saves copy of mesh in memory (Keep true if you need to access vertices in script)
                modelImporter.SaveAndReimport();
                Debug.Log($"Optimized Model: {path}");
            }
            // Optimize Textures
            else if (importer is TextureImporter textureImporter)
            {
                // Skip if it's not a texture type we want to compress (like a cursor)
                if (textureImporter.textureType == TextureImporterType.Default || 
                    textureImporter.textureType == TextureImporterType.Sprite)
                {
                    textureImporter.maxTextureSize = 1024; // Limit size (Coin doesn't need 4K)
                    textureImporter.textureCompression = TextureImporterCompression.Compressed;
                    textureImporter.crunchedCompression = true; // Use crunch compression for smaller size
                    textureImporter.compressionQuality = 50;    // Reasonable quality/size balance
                    textureImporter.SaveAndReimport();
                    Debug.Log($"Optimized Texture: {path}");
                }
            }
        }
        Debug.Log("Optimization Complete!");
    }
}
