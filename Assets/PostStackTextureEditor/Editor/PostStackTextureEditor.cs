using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.IO;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessTextureBaker : EditorWindow
{
    private Texture2D targetTexture;
    private string targetTextureFolder;
    private PostProcessProfile postProcessProfile;
    private TextureSelectionMode textureSelectionMode;

    RenderTexture processingRenderTexture;
    Camera renderCamera;
    PostProcessVolume targetPPVolume;
    PostProcessLayer targetPPLayer;
    GameObject textureMesh;
    Material targetMaterial;
    Texture2D finalTexture;

    enum TextureSelectionMode
    {
        Texture2D,
        // Folder,
        // VisibleInCamera
    };

    [MenuItem("Window/Rendering/Post Stack Texture Editor")]
    public static void ShowWindow()
    {
        GetWindow<PostProcessTextureBaker>(false, "Post Stack Texture Editor", true);
    }

    void OnGUI()
    {
        textureSelectionMode = (TextureSelectionMode)EditorGUILayout.EnumPopup("Texture Selection Mode", textureSelectionMode);
        

        if (textureSelectionMode == TextureSelectionMode.Texture2D)
        {
            targetTexture = (Texture2D)EditorGUILayout.ObjectField("Target Texture", targetTexture, typeof(Texture2D), false);
        }
        // else if (textureSelectionMode == TextureSelectionMode.Folder)
        // {
            // targetTextureFolder = EditorGUILayout.TextField("Target Texture Folder", targetTextureFolder);
        // }

        postProcessProfile = (PostProcessProfile)EditorGUILayout.ObjectField("Post Process Profile", postProcessProfile, typeof(PostProcessProfile), false);

        if (GUILayout.Button("Export Modified Texture(s)"))
        {
            BakeTexture();
        }
    }

    void BakeTexture()
    {
        if (!targetTexture)
        {
            Debug.Log("PostStackTextureEditor: targetTexture is missing");
            return;
        }

        if (!postProcessProfile)
        {
            Debug.Log("PostStackTextureEditor: postProcessProfile is missing");
            return;
        }

        processingRenderTexture = new RenderTexture(targetTexture.width, targetTexture.height, 0);
        RenderTexture.active = processingRenderTexture;

        renderCamera = new GameObject().AddComponent<Camera>();
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = targetTexture.height * 0.5f;
        renderCamera.targetTexture = processingRenderTexture;
        renderCamera.cullingMask = 4;

        targetPPVolume = renderCamera.gameObject.AddComponent<PostProcessVolume>();
        targetPPVolume.isGlobal = true;
        targetPPVolume.profile = postProcessProfile;
        targetPPLayer = renderCamera.gameObject.AddComponent<PostProcessLayer>();
        targetPPLayer.volumeLayer = 1;
        
        textureMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
        textureMesh.transform.position = renderCamera.transform.position + renderCamera.transform.forward * 50;
        textureMesh.transform.localScale = new Vector3(targetTexture.width, targetTexture.height, 1);
        textureMesh.layer = 2;

        targetMaterial = new Material(Shader.Find("Unlit/Texture"));
        textureMesh.GetComponent<MeshRenderer>().sharedMaterial = targetMaterial;
        textureMesh.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", targetTexture);

        renderCamera.Render();

        finalTexture = new Texture2D(targetTexture.width, targetTexture.height);
        finalTexture.ReadPixels(new Rect(0, 0, targetTexture.width, targetTexture.height), 0, 0);

        byte[] bytes = finalTexture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/" + targetTexture.name + ".png", bytes);

        AssetDatabase.Refresh();

        RenderTexture.active = null;
        renderCamera.targetTexture = null;

        DestroyImmediate(processingRenderTexture);
        DestroyImmediate(targetPPLayer);
        DestroyImmediate(targetPPVolume);
        DestroyImmediate(renderCamera.gameObject);
        DestroyImmediate(targetMaterial);
        DestroyImmediate(textureMesh);

        Debug.Log("PostStackTextureEditor: Texture Exported!");
    }
}
