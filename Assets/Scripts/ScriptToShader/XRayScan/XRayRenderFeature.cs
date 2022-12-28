using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class XRayRenderFeature : ScriptableRendererFeature
{
    private XRayRenderPass renderPass;
    public override void Create()
    {
        renderPass = new XRayRenderPass(RenderPassEvent.AfterRenderingOpaques);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
}
