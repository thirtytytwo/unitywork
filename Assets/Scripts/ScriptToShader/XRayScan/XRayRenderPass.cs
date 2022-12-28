using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class XRayRenderPass : ScriptableRenderPass
{
    private static readonly string renderTag = "XRay Scan";
    private static readonly int mainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int tempTargetID = Shader.PropertyToID("_TempTargetXRayScan");
    private static readonly int scanRangeID = Shader.PropertyToID("_ScanRange");
    private static readonly int scanDistanceID = Shader.PropertyToID("_ScanDistance");
    private static readonly int scanColorID = Shader.PropertyToID("_ScanColor");

    private float scanDistance = 0f;
    
    private Material scanMat;
    private ScanVolume volume;
    private RenderTargetIdentifier curTarget;

    public XRayRenderPass(RenderPassEvent evt)
    {
        renderPassEvent = evt;
        var shader = Shader.Find("PostEffect/XRayScan");
        if (shader == null)
        {
            Debug.LogError("Shader Not Found");
            return;
        }

        scanMat = CoreUtils.CreateEngineMaterial(shader);
    }

    public void Setup(in RenderTargetIdentifier target)
    {
        curTarget = target;
    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (scanMat == null)
        {
            Debug.LogError("Material Build Failed");
            return;
        }

        if (!renderingData.cameraData.postProcessEnabled) return;

        var stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<ScanVolume>();
        if (volume == null)
        {
            Debug.LogError("Can't get volume component");
            return;
        }
        if (!volume.IsActive()) return;
        
        var cmd = CommandBufferPool.Get(renderTag);
        Render(cmd, ref renderingData);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var data = ref renderingData.cameraData;
        var source = curTarget;
        int dest = tempTargetID;

        var w = data.camera.scaledPixelWidth;
        var h = data.camera.scaledPixelHeight;
        int shaderPass = 0;

        scanDistance = Mathf.Lerp(scanDistance, 1, Time.deltaTime * volume.updateSpeed.value);

        if (scanDistance > 0.9f) scanDistance = 0f;
        
        scanMat.SetFloat(scanDistanceID, scanDistance);
        scanMat.SetFloat(scanRangeID, volume.scanRange.value);
        scanMat.SetColor(scanColorID, volume.scanColor.value);
        
        cmd.SetGlobalTexture(mainTexID, source);
        cmd.GetTemporaryRT(dest,w,h,0,FilterMode.Bilinear,RenderTextureFormat.Default);
        cmd.Blit(source,dest);
        cmd.Blit(dest,source, scanMat,shaderPass);
    }
}
