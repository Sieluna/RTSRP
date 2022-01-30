using UnityEngine;
using UnityEngine.Rendering;

public class BackgroundTutorial : RayTracingTutorial
{
    public BackgroundTutorial(RayTracingTutorialAsset asset) : base(asset) { }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
        base.Render(context, camera);
        var outputTarget = RequireOutputTarget(camera);

        var cmd = CommandBufferPool.Get(nameof(BackgroundTutorial));
        try
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
            {
                cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
                cmd.DispatchRays(m_Shader, "BackgroundRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
            }

            context.ExecuteCommandBuffer(cmd);

            using (new ProfilingScope(cmd, new ProfilingSampler("FinalBlit")))
            {
                cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
            }

            context.ExecuteCommandBuffer(cmd);
        }
        finally
        {
            CommandBufferPool.Release(cmd);
        }
    }
}