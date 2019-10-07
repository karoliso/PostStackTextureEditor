using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.IO;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessTextureBaker : EditorWindow
{
    public Texture2D[] targetTextures;
    public string targetTextureFolder;
    private PostProcessProfile postProcessProfile;
    private TextureSelectionMode textureSelectionMode;

    RenderTexture processingRenderTexture;
    Camera renderCamera;
    PostProcessVolume targetPPVolume;
    PostProcessLayer targetPPLayer;
    GameObject textureMesh;
    Material targetMaterial;
    Texture2D finalTexture;

    private SerializedObject so;
    private SerializedProperty stringsProp;

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

    private void OnEnable()
    {
        so = new SerializedObject(this);
        stringsProp = so.FindProperty("targetTextures");
    }

    void OnGUI()
    {
        textureSelectionMode = (TextureSelectionMode)EditorGUILayout.EnumPopup("Texture Selection Mode", textureSelectionMode);

        if (textureSelectionMode == TextureSelectionMode.Texture2D)
        {
            EditorGUILayout.PropertyField(stringsProp, true);
            so.ApplyModifiedProperties();
        }
        //else if (textureSelectionMode == TextureSelectionMode.Folder)
        //{
        //    EditorGUILayout.PropertyField(stringsProp, true);
        //    so.ApplyModifiedProperties();
        //    targetTextureFolder = EditorGUILayout.TextField("Target Texture Folder", targetTextureFolder);
        //}

        postProcessProfile = (PostProcessProfile)EditorGUILayout.ObjectField("Post Process Profile", postProcessProfile, typeof(PostProcessProfile), false);

        if (GUILayout.Button("Export Modified Texture(s)"))
        {
            BakeTexture();
        }
    }

    void BakeTexture()
    {
        if (targetTextures.Length <= 0)
        {
            Debug.Log("PostStackTextureEditor: no targetTextures");
            return;
        }

        if (!postProcessProfile)
        {
            Debug.Log("PostStackTextureEditor: postProcessProfile is missing");
            return;
        }

        renderCamera = new GameObject().AddComponent<Camera>();
        renderCamera.orthographic = true;
        renderCamera.cullingMask = 4;

        targetPPVolume = renderCamera.gameObject.AddComponent<PostProcessVolume>();
        targetPPVolume.isGlobal = true;
        targetPPVolume.profile = postProcessProfile;
        targetPPLayer = renderCamera.gameObject.AddComponent<PostProcessLayer>();
        targetPPLayer.volumeLayer = 1;

        textureMesh = GameObject.CreatePrimitive(PrimitiveType.Quad);
        textureMesh.layer = 2;

        targetMaterial = new Material(Shader.Find("Unlit/Texture"));
        textureMesh.GetComponent<MeshRenderer>().sharedMaterial = targetMaterial;

        for (int i = 0; i < targetTextures.Length; i++)
        {
            if (!targetTextures[i])
            {
                continue;
            }

            processingRenderTexture = new RenderTexture(targetTextures[i].width, targetTextures[i].height, 0);
            RenderTexture.active = processingRenderTexture;

            renderCamera.orthographicSize = targetTextures[i].height * 0.5f;
            renderCamera.targetTexture = processingRenderTexture;

            textureMesh.transform.position = renderCamera.transform.position + renderCamera.transform.forward * 50;
            textureMesh.transform.localScale = new Vector3(targetTextures[i].width, targetTextures[i].height, 1);
            
            textureMesh.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_MainTex", targetTextures[i]);

            renderCamera.Render();

            finalTexture = new Texture2D(targetTextures[i].width, targetTextures[i].height);
            finalTexture.ReadPixels(new Rect(0, 0, targetTextures[i].width, targetTextures[i].height), 0, 0);

            byte[] bytes = finalTexture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/" + targetTextures[i].name + ".png", bytes);
        }

        RenderTexture.active = null;
        renderCamera.targetTexture = null;

        DestroyImmediate(processingRenderTexture);
        DestroyImmediate(targetPPLayer);
        DestroyImmediate(targetPPVolume);
        DestroyImmediate(renderCamera.gameObject);
        DestroyImmediate(targetMaterial);
        DestroyImmediate(textureMesh);

        AssetDatabase.Refresh();

        Debug.Log("PostStackTextureEditor: Texture Exported!");
    }
}
