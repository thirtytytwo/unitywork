using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FogAreaItem
{
    public int index = 0;
    public Color color;
    public void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        index = EditorGUILayout.IntField("迷雾序号(1-254)(不可重复)", index, GUILayout.MaxWidth(200));
        color = EditorGUILayout.ColorField(color, GUILayout.MinWidth(100));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("开始编辑此区域"))
        {
            FogManager.GetInstance().curIndex = index;
            FogManager.GetInstance().curColor = color;
        }
        if (GUILayout.Button("删除此区域"))
        {
            foreach(var t in FogManager.GetInstance().allTile)
            {
                if(t.index == index)
                {
                    t.index = 0;
                    t.gameObject.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 0.75f);
                }
            }
            foreach(var t in FogManager.GetInstance().allFogEditor)
            {
                if(t.index == index)
                {
                    FogManager.GetInstance().allFogEditor.Remove(t);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
