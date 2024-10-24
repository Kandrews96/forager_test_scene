using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
[Serializable, VolumeComponentMenu("Custom Post Processing/Pixelate")]
public class PixelateSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Size of each new 'pixel' in the image")]
    public ClampedIntParameter pixelSize = new ClampedIntParameter(1,1,256);

    public bool IsActive()
    {
        return pixelSize.value > 1 && active;
    }

    public bool IsTileCompatible()
    {
        throw new System.NotImplementedException();
    }
}
