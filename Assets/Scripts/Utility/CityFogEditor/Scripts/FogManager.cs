using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class FogManager
{
    private static FogManager instance = new FogManager();
    FogManager() { }
    public static FogManager GetInstance()
    {
        return instance;
    }
    //需要传入编辑器数据
    public string savePath;
    public string saveName;
    private Transform root;
    public int texSize;
    public int searchSize = 1;//检查边界和相邻范围
    public float paintRadius = 0;
    //Manager类数据
    public int curIndex = 0;
    public Color curColor;
    public List<FogAreaItem> allFogEditor = new List<FogAreaItem>();//迷雾区域容器
    public List<TileInfo> allTile = new List<TileInfo>();//存放格子数据容器

    //图片通道数据
    int[,] indexData;//存放像素对应掩码序号
    int[,] limitData;//边界
    int[,] nearData;//像素周边区域对应序号
    //临时数据
    int[,] dir = new int[,] { { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };

    #region 编辑器写入数据
    //初始化迷雾编辑变量
    public void FogInit()
    {
        //生成父类层级存放
        root = new GameObject("tile-root").transform;
    }
    //清理网格
    public void ClearEdit()
    {
        if (root != null) GameObject.DestroyImmediate(root.gameObject);
    }
    //生成网格
    public void GenerateGrid(GameObject tile, GameObject grid)
    {
        GameObject obj = GameObject.Instantiate(grid, new Vector3(-0.5f, 0, -0.5f), Quaternion.identity, root);
        obj.transform.localScale = new Vector3(texSize, 0.1f, texSize);
        obj.transform.GetComponent<Renderer>().sharedMaterial.SetTextureScale("_BaseMap", new Vector2(texSize, texSize));
        for (int i = -texSize / 2; i < texSize / 2; ++i)
        {
            for (int j = -texSize / 2; j < texSize / 2; ++j)
            {
                GameObject t = GameObject.Instantiate(tile, new Vector3(i, 0, j), Quaternion.identity, root);
                t.GetComponent<TileInfo>().pos = new Vector2Int(i + texSize / 2, j + texSize / 2);
                allTile.Add(t.GetComponent<TileInfo>());
            }
        }
    }
    //改变网格掩码值
    public void DrawTileIdx()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == "Tile(Clone)")
            {
                hit.transform.GetComponent<TileInfo>().index = curIndex;
                var m0 = new Material(hit.transform.GetComponent<Renderer>().sharedMaterial);
                m0.color = new Color(curColor.r, curColor.g, curColor.b, 0.75f);
                hit.transform.GetComponent<Renderer>().sharedMaterial = m0;
                if (paintRadius > 0)
                {
                    Collider[] cols = Physics.OverlapSphere(hit.transform.position, paintRadius);
                    if (cols != null)
                    {
                        foreach (var col in cols)
                        {
                            col.transform.GetComponent<TileInfo>().index = curIndex;
                            var m = new Material(col.transform.GetComponent<Renderer>().sharedMaterial);
                            m.color = new Color(curColor.r, curColor.g, curColor.b, 0.75f);
                            col.transform.GetComponent<Renderer>().sharedMaterial = m;
                        }
                    }
                }
            }
        }
    }
    #endregion
    #region 解析数据并导出
    //初始化图片数据
    public void InitTexData()
    {
        indexData = new int[texSize, texSize];
        for (int i = 0; i < texSize; ++i)
        {
            for (int j = 0; j < texSize; ++j)
            {
                indexData[i, j] = 0;
            }
        }
        limitData = new int[texSize, texSize];
        for (int i = 0; i < texSize; ++i)
        {
            for (int j = 0; j < texSize; ++j)
            {
                limitData[i, j] = 0;
            }
        }
        nearData = new int[texSize, texSize];
        for (int i = 0; i < texSize; ++i)
        {
            for (int j = 0; j < texSize; ++j)
            {
                nearData[i, j] = 0;
            }
        }
    }
    public void DrawR()//临时
    {
        var array = allTile.ToArray();
        foreach (var item in array)
        {
            indexData[item.pos.x, item.pos.y] = item.index;
        }
    }
    //检查边界(G通道)
    public void CheckLimitForG()
    {
        //遍历检查周围八个方向是否有不同与该序号的区域序号，如果有就是边界
        for (int i = 0; i < texSize; ++i)
        {
            for (int j = 0; j < texSize; ++j)
            {
                int curidx = indexData[i, j];
                //不是空地才判断边界
                if (curidx != 0 && curidx != 255)
                {
                    for (int k = 0; k < dir.Length / 2; ++k)
                    {
                        if (i + dir[k, 0] * searchSize >= texSize || i + dir[k, 0] * searchSize < 0 || j + dir[k, 1] * searchSize >= texSize || j + dir[k, 1] * searchSize < 0) continue;
                        if (indexData[i + dir[k, 0] * searchSize, j + dir[k, 1]] * searchSize != curidx)
                        {
                            limitData[i, j] = curidx;
                            break;
                        }
                    }
                }
            }
        }
    }
    //检查相邻区域(B通道)
    public void CheckAroundForB()
    {
        for (int i = 0; i < texSize; ++i)
        {
            for (int j = 0; j < texSize; ++j)
            {
                int curidx = indexData[i, j];
                //不是空地才判断边界
                if (curidx != 0 && curidx != 255)
                {
                    for (int k = 0; k < dir.Length / 2; ++k)
                    {
                        if (i + dir[k, 0] * searchSize >= texSize || i + dir[k, 0] * searchSize < 0 || j + dir[k, 1] * searchSize >= texSize || j + dir[k, 1] * searchSize < 0) continue;
                        if (indexData[i + dir[k, 0] * searchSize, j + dir[k, 1] * searchSize] != curidx && indexData[i + dir[k, 0] * searchSize, j + dir[k, 1] * searchSize] != 255 && indexData[i + dir[k, 0] * searchSize, j + dir[k, 1] * searchSize] != 0)
                        {
                            nearData[i, j] = indexData[i + dir[k, 0] * searchSize, j + dir[k, 1] * searchSize];
                            break;
                        }
                    }
                }
            }
        }
    }
    //生成点采样图片并保存
    public void GenerateTexAndSave()
    {
        Texture2D tex = new Texture2D(texSize, texSize, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        for (int i = 0; i < texSize; ++i)
        {
            for (int j = 0; j < texSize; ++j)
            {
                //写入图片
                tex.SetPixel(i, j, new Color(indexData[i, j], 0, 0));//, limitData[i, j] / 255f, nearData[i, j] / 255f));
            }
        }
        tex.filterMode = FilterMode.Point;
        tex.Apply();
        //导出为bytes
        byte[] bytes = tex.GetRawTextureData();
        Object.DestroyImmediate(tex);
        if (saveName == null)
        {
            Debug.LogError("请输入正确的输出文件名称");
        }
        File.WriteAllBytes(savePath + "/" + saveName + ".bytes", bytes);
    }
    #endregion
}
