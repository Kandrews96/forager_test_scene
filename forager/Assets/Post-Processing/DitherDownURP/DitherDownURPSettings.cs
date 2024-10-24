using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenuForRenderPipeline("Custom Post Processing/Dither Down URP", typeof(UniversalRenderPipeline))]
public class DitherDownURPSettings : VolumeComponent, IPostProcessComponent
{

    #region Local enum and wrapper class
        public enum DitherType { Bayer2x2, Bayer3x3, Bayer4x4, Bayer8x8 }
        [Serializable] public sealed class DitherTypeParameter : VolumeParameter<DitherType> {}
    #endregion

    public BoolParameter enabled = new BoolParameter(false);
    
    public ClampedIntParameter colorLevels = new ClampedIntParameter(16, 1, 24);
    public DitherTypeParameter ditherType = new DitherTypeParameter { value = DitherType.Bayer2x2 };
    public ClampedFloatParameter dithering = new ClampedFloatParameter(0f, 0, 0.5f);
    public ClampedFloatParameter downsampling = new ClampedFloatParameter(1.0f, 1.0f, 32.0f);

    public bool IsActive()
    {
        return enabled.value && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
