using UnityEditor;
using UnityEngine;
using System.IO;

public class ItemIconBaker : EditorWindow
{
    public Camera bakeCamera;
    public Transform modelRoot;
    public int iconSize = 256; // 图标尺寸，推荐256/512，2的整数次幂
    public string savePath = "Assets/Sprites/ItemIcons/"; // 保存路径

    // 打开编辑器窗口
    [MenuItem("Tools/物品图标烘焙工具")]
    public static void ShowWindow()
    {
        GetWindow<ItemIconBaker>("物品图标烘焙工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("烘焙基础设置", EditorStyles.boldLabel);
        bakeCamera = EditorGUILayout.ObjectField("烘焙相机", bakeCamera, typeof(Camera), true) as Camera;
        modelRoot = EditorGUILayout.ObjectField("模型挂载点", modelRoot, typeof(Transform), true) as Transform;
        iconSize = EditorGUILayout.IntField("图标尺寸", iconSize);
        savePath = EditorGUILayout.TextField("保存路径", savePath);

        if (GUILayout.Button("烘焙当前模型图标", GUILayout.Height(30)))
        {
            BakeCurrentIcon();
        }
    }

    // 核心烘焙方法
    private void BakeCurrentIcon()
    {
        // 校验参数
        if (bakeCamera == null || modelRoot == null)
        {
            EditorUtility.DisplayDialog("错误", "请赋值烘焙相机和模型挂载点", "确定");
            return;
        }

        // 创建文件夹
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // 创建RenderTexture
        RenderTexture rt = new RenderTexture(iconSize, iconSize, 32, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 1;
        bakeCamera.targetTexture = rt;

        // 渲染相机画面
        Texture2D bakedTex = new Texture2D(iconSize, iconSize, TextureFormat.ARGB32, false);
        bakeCamera.Render();
        RenderTexture.active = rt;

        // 读取渲染像素，保留透明通道
        bakedTex.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
        bakedTex.Apply();

        // 保存为PNG
        byte[] pngData = bakedTex.EncodeToPNG();
        string modelName = modelRoot.GetChild(0).name;
        string fullPath = savePath + modelName + "_Icon.png";
        File.WriteAllBytes(fullPath, pngData);

        // 释放资源
        RenderTexture.active = null;
        bakeCamera.targetTexture = null;
        DestroyImmediate(rt);
        DestroyImmediate(bakedTex);

        // 刷新资源
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("成功", $"图标已保存至：{fullPath}", "确定");
    }
}