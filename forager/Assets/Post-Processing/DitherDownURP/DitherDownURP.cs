
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherDownURP : ScriptableRendererFeature
{
    
    internal class DitherDownURPPass : ScriptableRenderPass
    {
        #region Local enum and wrapper class
        public enum DitherType { Bayer2x2, Bayer3x3, Bayer4x4, Bayer8x8 }
        [Serializable] public sealed class DitherTypeParameter : VolumeParameter<DitherType> {}
    #endregion


    static class IDs
    {
        internal static readonly int Dithering = Shader.PropertyToID("_Dithering");
        internal static readonly int Downsampling = Shader.PropertyToID("_Downsampling");
        //internal static readonly int InputTexture = Shader.PropertyToID("_InputTex");
        internal static readonly int DitherTexture = Shader.PropertyToID("_DitherTexture");
        internal static readonly int Levels = Shader.PropertyToID("_Levels");
    }

        Material material;
        DitherDownURPSettings settings;
        private RenderTextureDescriptor tempTexDescriptor;
        private RTHandle tempTexHandle;
        private string profilerTag;

        public DitherDownURPPass(Material material)
        {
            this.material = material;
            profilerTag = "DitherDownURP";
            settings = VolumeManager.instance.stack.GetComponent<DitherDownURPSettings>();
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void EnqueuePass(ScriptableRenderer renderer)
        {
            if (settings != null && settings.IsActive())
            {
                renderer.EnqueuePass(this);
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (settings == null)
            {
                return;
            }

            tempTexDescriptor = cameraTextureDescriptor;
            tempTexDescriptor.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref tempTexHandle, tempTexDescriptor);


            
            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(!settings.IsActive())
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            
            if(material == null) { return; }


            material.SetFloat(IDs.Dithering, settings.dithering.value);
            material.SetFloat(IDs.Levels, settings.colorLevels.value);
            material.SetFloat(IDs.Downsampling, settings.downsampling.value);
            material.SetTexture(IDs.DitherTexture, GenerateDitherTexture((DitherType)settings.ditherType.value));
            //material.SetTexture(IDs.InputTexture, tempTexHandle);
            
            RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;
           
    
            

            using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
            {
                Blit(cmd, source, tempTexHandle); 
                Blit(cmd, tempTexHandle, source, material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying)
            {
                 Destroy(material);
            }
            else
            {
                DestroyImmediate(material);
            }
#else
            Destroy(material);
#endif
            tempTexHandle?.Release();
        } 

        #region Dither texture generator

        static Texture2D GenerateDitherTexture(DitherType type)
        {
            if (type == DitherType.Bayer2x2)
            {
                var tex = new Texture2D(2, 2, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {0, 170, 255, 85});
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer3x3) 
            {
                var tex = new Texture2D(3, 3, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {
                    0, 223, 95, 191, 159, 63, 127, 31, 255
                });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer4x4)
            {
                var tex = new Texture2D(4, 4, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {
                    0, 136, 34, 170, 204, 68, 238, 102,
                    51, 187, 17, 153, 255, 119, 221, 85
                });
                tex.Apply();
                return tex;
            }

            if (type == DitherType.Bayer8x8)
            {
                var tex = new Texture2D(8, 8, TextureFormat.R8, false, true);
                tex.LoadRawTextureData(new byte [] {
                    0, 194, 48, 242, 12, 206, 60, 255,
                    129, 64, 178, 113, 141, 76, 190, 125,
                    32, 226, 16, 210, 44, 238, 28, 222,
                    161, 97, 145, 80, 174, 109, 157, 93,
                    8, 202, 56, 250, 4, 198, 52, 246,
                    137, 72, 186, 121, 133, 68, 182, 117,
                    40, 234, 24, 218, 36, 230, 20, 214,
                    170, 105, 153, 89, 165, 101, 149, 85
                });
                tex.Apply();
                return tex;
            }

            return null;
        }

        #endregion
    }

    DitherDownURPPass pass;

    public override void Create()
    {
        var shader = Shader.Find("CustomPostProcessing/DitherDownURP");
        if (shader == null)
        {
            Debug.LogError("CustomPostProcessing/DitherDownURP shader not found.");
            return;
        }

        var material = CoreUtils.CreateEngineMaterial(shader);

        pass = new DitherDownURPPass(material);
        name = "Dither Down URP";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            pass.EnqueuePass(renderer);
        }
    }

    protected override void Dispose(bool disposing)
    {
        pass.Dispose();
        base.Dispose(disposing);
    }
}
