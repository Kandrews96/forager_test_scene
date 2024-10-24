using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Pixelate : ScriptableRendererFeature
{

    public class PixelateRenderPass : ScriptableRenderPass
    {
        private PixelateSettings m_Settings;
        private RenderTextureDescriptor m_tempTexDescriptor;
        private RTHandle M_tempTexHandle;
        private string m_profilerTag;

        public PixelateRenderPass()
        {
            m_Settings = VolumeManager.instance.stack.GetComponent<PixelateSettings>();
            m_profilerTag = "Pixelate";
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public void EnqueuePass(ScriptableRenderer renderer)
        {
            if(m_Settings != null && m_Settings.IsActive())
            {
                renderer.EnqueuePass(this);
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if(m_Settings == null)
            {
                return;
            }

            int width = cameraTextureDescriptor.width / m_Settings.pixelSize.value;
            int height = cameraTextureDescriptor.height / m_Settings.pixelSize.value;

            m_tempTexDescriptor = cameraTextureDescriptor;
            m_tempTexDescriptor.width = width;
            m_tempTexDescriptor.height = height;
            m_tempTexDescriptor.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref M_tempTexHandle, m_tempTexDescriptor);
            
            base.Configure(cmd, cameraTextureDescriptor);
        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(!m_Settings.IsActive())
            {
                return;
            }

            //no shader properties to set here - we're simply going to Blit the new pixelised pixel size


            CommandBuffer cmd = CommandBufferPool.Get(m_profilerTag);
            RTHandle cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            using (new ProfilingScope(cmd, new ProfilingSampler(m_profilerTag)))
            {
                Blit(cmd, cameraTargetHandle, M_tempTexHandle);
                Blit(cmd, M_tempTexHandle, cameraTargetHandle);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            M_tempTexHandle?.Release();
        }
    }

    

    PixelateRenderPass pass;

    public override void Create()
    {
        pass = new PixelateRenderPass();
        name = "Pixelate";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.EnqueuePass(renderer);
    }

    protected override void Dispose(bool disposing)
    {
        pass.Dispose();
        base.Dispose(disposing);
    }


}
