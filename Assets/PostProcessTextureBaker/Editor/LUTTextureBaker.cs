using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.IO;

public class PostProcessTextureBaker : EditorWindow
{
    private Texture2D targetTexture;
    private Texture2D LUTTexture;
    private const string LUTShaderName = "Hidden/LUTColorGrading";

    [MenuItem("Window/Rendering/LUT Texture Baker")]
    public static void ShowWindow()
    {
        GetWindow<PostProcessTextureBaker>(false, "LUT Texture Baker", true);
    }

    void OnGUI()
    {
        targetTexture = (Texture2D)EditorGUILayout.ObjectField("Target Texture", targetTexture, typeof(Texture2D), false);
        LUTTexture = (Texture2D)EditorGUILayout.ObjectField("LUT Texture", LUTTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Bake LUT to Texture"))
        {
            BakeLUT();
        }
    }

    void BakeLUT()
    {
        if (!targetTexture)
        {
            Debug.LogWarning("LUT Texture Baker: no targetTexture specified.");
            return;
        }

        if (!LUTTexture)
        {
            Debug.LogWarning("LUT Texture Baker: no LUTTexture specified.");
            return;
        }

        Shader LUTShader = Shader.Find(LUTShaderName);
        Material LUTMaterial = new Material(LUTShader);
        LUTMaterial.SetTexture("_LUT", LUTTexture);
        RenderTexture processingRenderTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);
        Graphics.Blit(targetTexture, processingRenderTexture, LUTMaterial);

        RenderTexture.active = processingRenderTexture;
        Texture2D finalTexture = new Texture2D(targetTexture.width, targetTexture.height);
        finalTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);

        byte[] bytes = finalTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/" + targetTexture.name + ".png", bytes);

        AssetDatabase.Refresh();
    }
}
