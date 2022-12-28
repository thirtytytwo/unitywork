using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class IndexTextureEditor : EditorWindow
{
    [MenuItem("Tool/ Init IndexTex With Json")]
    public static void Open()
    {
        EditorWindow.GetWindow(typeof(IndexTextureEditor), true);
    }
    //编辑器数据
    private bool active2Edit = false;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUICallBack;
    }
    private void OnGUI()
    {
        FogManager.GetInstance().texSize = EditorGUILayout.IntField("输出图片尺寸", FogManager.GetInstance().texSize, GUILayout.MaxWidth(200));
        FogManager.GetInstance().searchSize = EditorGUILayout.IntField("规定相邻范围", FogManager.GetInstance().searchSize, GUILayout.MaxWidth(200));
        FogManager.GetInstance().paintRadius = EditorGUILayout.FloatField("绘制范围(如需要)", FogManager.GetInstance().paintRadius, GUILayout.MaxWidth(200));
        FogManager.GetInstance().saveName = EditorGUILayout.TextField("输出图片名字", FogManager.GetInstance().saveName, GUILayout.MinWidth(100));
        FogManager.GetInstance().savePath = EditorGUILayout.TextField("保存图片路径", FogManager.GetInstance().savePath, GUILayout.MinWidth(100));
        EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
        if(GUILayout.Button("初始化网格", GUILayout.MaxWidth(150)))
        {
            FogManager.GetInstance().FogInit();
            FogManager.GetInstance().GenerateGrid(AssetDatabase.LoadAssetAtPath("Assets/CityFogEditor/Prefab/Tile.prefab", typeof(GameObject)) as GameObject,
                                                  AssetDatabase.LoadAssetAtPath("Assets/CityFogEditor/Prefab/Grid.prefab", typeof(GameObject)) as GameObject);
        }
        if (GUILayout.Button("开始编辑",GUILayout.MaxWidth(150)))
        {
            active2Edit = true;
        }
        if(GUILayout.Button("新增迷雾区域", GUILayout.MaxWidth(150)))
        {
            FogAreaItem item = new FogAreaItem();
            FogManager.GetInstance().allFogEditor.Add(item);
        }
        if(GUILayout.Button("退出编辑", GUILayout.MaxWidth(150)))
        {
            active2Edit = false;
        }
        EditorGUILayout.EndHorizontal();
        foreach(var item in FogManager.GetInstance().allFogEditor)
        {
            item.OnGUI();
        }
        if (GUILayout.Button("导出"))
        {
            FogManager.GetInstance().InitTexData();
            FogManager.GetInstance().DrawR();
            FogManager.GetInstance().CheckLimitForG();
            FogManager.GetInstance().CheckAroundForB();
            FogManager.GetInstance().GenerateTexAndSave();
        }
    }
    private void OnSceneGUICallBack(SceneView scene)
    {
        //进入编辑模式
        if (active2Edit)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (Event.current.button == 0 && Event.current.type == EventType.MouseDrag)
            {
                FogManager.GetInstance().DrawTileIdx();
            }
        }
        else
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));
        }
    }
    private void OnDisable()
    {
        FogManager.GetInstance().ClearEdit();
    }
}