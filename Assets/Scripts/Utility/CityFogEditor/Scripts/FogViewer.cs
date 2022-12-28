using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FogViewer: MonoBehaviour
{
    public int texSize;
    public int highlight_Index;
    //数据
    float[] unlockData;//存放已解锁的序号

    //图形与材质
    public Texture2D tex;
    public Material curMat;

    //抠图
    public Shader getActiveIndexShader = null;
    //当前和之前结果混合
    public Shader blendShader;

    //实现高斯模糊
    public Shader blurShader = null;
    public int blurTimes = 2;

    //RT
    public RenderTexture retRT_active;
    public RenderTexture retRT_highlight;
    private RenderTexture rt_getIndex_r;
    private RenderTexture rt_gethighlight;
    private RenderTexture rt_blend;

    private void Start()
    {
        //初始化数据
        tex = new Texture2D(texSize, texSize,UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        //初始化rt
        retRT_active = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.R8);
        retRT_highlight = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.R8);
        rt_getIndex_r = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.R8);
        rt_blend = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.R8);
        rt_gethighlight = new RenderTexture(texSize, texSize, 0, RenderTextureFormat.R8);
        //解析bytes
        byte[] content = File.ReadAllBytes(Application.dataPath + "/test1.bytes");
        tex.filterMode = FilterMode.Point;
        tex.LoadRawTextureData(content);
        tex.Apply();
        unlockData = new float[256];
        for(int i = 0; i < unlockData.Length; ++i)
        {
            unlockData[i] = 0;
        }
        //获取材质
        curMat = GetComponent<Renderer>().material;

    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Keypad1)) GenerateBlinearTex(1, 1);
        if (Input.GetKeyDown(KeyCode.Keypad2)) GenerateBlinearTex(0, 2);
        if (Input.GetKeyDown(KeyCode.Keypad3)) GenerateBlinearTex(0, 3);
    }

    public void UpdateCurrentHighlightArea(int index)
    {
        highlight_Index = index;
        GenerateBlinearTex(0,2);
        GenerateBlinearTex(0,1);
    }

    private RenderTexture Blur(RenderTexture _tex)
    {
        RenderTexture rt0 = RenderTexture.GetTemporary(texSize, texSize, 0, RenderTextureFormat.R8);
        RenderTexture rt1 = RenderTexture.GetTemporary(texSize, texSize, 0, RenderTextureFormat.R8);
        Material blurMat = new Material(blurShader);
        blurMat.SetVector("_TexSize", new Vector4(texSize, texSize));
        Graphics.Blit(_tex, rt0);
        for(int i = 0; i < blurTimes; ++i)
        {
            Graphics.Blit(rt0, rt1, blurMat, 0);
            Graphics.Blit(rt1, rt0, blurMat, 1);
        }
        return rt0;
    }
    private void Blend()
    {
        Material blendMat = new Material(blendShader);
        blendMat.SetTexture("_CurTex", rt_getIndex_r);
        Graphics.Blit(rt_blend, retRT_active, blendMat, 0);
        Graphics.Blit(retRT_active, rt_blend);
    }
    private void GenerateBlinearTex(int channel, int index)
    {
        Material indexMat = new Material(getActiveIndexShader);
        indexMat.SetInt("_Channel", channel);
        indexMat.SetInt("_Index", index);
        //解锁区域操作
        if(channel == 0)
        {
            Graphics.Blit(tex, rt_getIndex_r, indexMat, 0);
            Blend();
            curMat.SetTexture("_BlurTexRuntime", Blur(retRT_active));
        }
        else if(channel == 1 && unlockData[index] == 0)
        {
            indexMat.SetTexture("_LastTex", rt_gethighlight);
            Graphics.Blit(tex, retRT_highlight, indexMat, 0);
            Graphics.Blit(retRT_highlight, rt_gethighlight);
            curMat.SetTexture("_BlurTexRuntime_Limit", Blur(retRT_highlight));
        }
    }
    /// <summary>
    /// 更新数据：1~100R通道数据 101~200G通道数据
    /// 输入时G通道为原先值加100
    /// 输入参数 channel 更新解锁区域:0 更新点击发光区域:1
    /// </summary>
    /// <param name="index"></param>
    public void UpdateUnlockData(int index, int channel)
    {
        unlockData[index] = 1;
        //GenerateBlinearTex(channel);
    }
}
