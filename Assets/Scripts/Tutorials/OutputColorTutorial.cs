using UnityEngine;
using UnityEngine.Rendering;

public class OutputColorTutorial : RayTracingTutorial
{
    public OutputColorTutorial(RayTracingTutorialAsset asset) : base(asset) { }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
        base.Render(context, camera);
        var outputTarget = RequireOutputTarget(camera);

        var cmd = CommandBufferPool.Get(nameof(OutputColorTutorial));
        try
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
            {
                cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
                cmd.DispatchRays(m_Shader, "OutputColorRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
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