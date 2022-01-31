using UnityEngine;
using UnityEngine.Rendering;

public class CreateSphereTutorial : RayTracingTutorial
{
    public CreateSphereTutorial(RayTracingTutorialAsset asset) : base(asset) { }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
        base.Render(context, camera);
        var outputTarget = RequireOutputTarget(camera);

        var accelerationStructure = m_Pipeline.RequestAccelerationStructure();

        var cmd = CommandBufferPool.Get(nameof(CreateSphereTutorial));
        try
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
            {
                cmd.SetRayTracingShaderPass(m_Shader, "RayTracing");
                cmd.SetRayTracingAccelerationStructure(m_Shader, RayTracingRenderPipeline.s_AccelerationStructure, accelerationStructure);
                cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
                cmd.DispatchRays(m_Shader, "CreateSphereRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
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