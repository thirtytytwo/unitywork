using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScanVolume : VolumeComponent ,IPostProcessComponent
{
    [Tooltip("效果开关")]
    public BoolParameter openSwitch = new BoolParameter(false);
    public FloatParameter updateSpeed = new FloatParameter(1f);
    public ColorParameter scanColor = new ColorParameter(Color.white);
    public FloatParameter scanRange = new FloatParameter(1f);
    
    public bool IsActive() => openSwitch.value;
    public bool IsTileCompatible() => false;
}
