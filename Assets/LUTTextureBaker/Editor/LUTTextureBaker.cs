using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.IO;
using Unity.Jobs;

public class LUTTextureBaker : EditorWindow
{
    private Texture2D targetTexture;
    private Texture2D LUTTexture;
    private Material shadingMaterial;
    private Shader LUTShader;

    private Material resultMat;

    [MenuItem("Window/Rendering/LUT Texture Baker")]
    public static void ShowWindow()
    {
        GetWindow<LUTTextureBaker>(false, "LUT Texture Baker", true);
    }

    void OnGUI()
    {
        targetTexture = (Texture2D)EditorGUILayout.ObjectField("Target Texture", targetTexture, typeof(Texture2D), false);
        LUTTexture = (Texture2D)EditorGUILayout.ObjectField("LUT Texture", LUTTexture, typeof(Texture2D), false);
        shadingMaterial = (Material)EditorGUILayout.ObjectField("Shading Material", shadingMaterial, typeof(Material), false);
        LUTShader = (Shader)EditorGUILayout.ObjectField("LUT Shader", LUTShader, typeof(Shader), false);

        resultMat = (Material)EditorGUILayout.ObjectField("Result Mat", resultMat, typeof(Material), false);

        if (GUILayout.Button("Bake LUT to Texture"))
        {
            BakeLUT();
        }
    }

    void BakeLUT()
    {
        shadingMaterial.shader = LUTShader;
        shadingMaterial.SetTexture("_LUT", LUTTexture);
        RenderTexture processingRenderTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);
        Graphics.Blit(targetTexture, processingRenderTexture, shadingMaterial);

        resultMat.SetTexture("_MainTex", processingRenderTexture);

        RenderTexture.active = processingRenderTexture;
        Texture2D finalTexture = new Texture2D(targetTexture.width, targetTexture.height);
        finalTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);

        byte[] bytes = finalTexture.EncodeToPNG();

        File.WriteAllBytes(Application.dataPath + "/" + "LUTCorrectedTexture" + ".png", bytes);

        AssetDatabase.Refresh();
    }
}
