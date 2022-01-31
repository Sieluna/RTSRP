using UnityEngine;
using UnityEngine.Rendering;

public class AntialiasingTutorial : RayTracingTutorial
{
    private static readonly int s_FrameIndex = Shader.PropertyToID("_FrameIndex");
    private static readonly int s_PRNGStates = Shader.PropertyToID("_PRNGStates");

    private int m_FrameIndex;

    public AntialiasingTutorial(RayTracingTutorialAsset asset) : base(asset) { }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
        base.Render(context, camera);
        var outputTarget = RequireOutputTarget(camera);
        var outputTargetSize = RequireOutputTargetSize(camera);

        var accelerationStructure = m_Pipeline.RequestAccelerationStructure();
        var PRNGStates = m_Pipeline.RequirePRNGStates(camera);

        var cmd = CommandBufferPool.Get(nameof(AntialiasingTutorial));
        try
        {
            if (m_FrameIndex < 1000)
            {
                using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
                {
                    cmd.SetRayTracingShaderPass(m_Shader, "RayTracing");
                    cmd.SetRayTracingAccelerationStructure(m_Shader, RayTracingRenderPipeline.s_AccelerationStructure, accelerationStructure);
                    cmd.SetRayTracingIntParam(m_Shader, s_FrameIndex, m_FrameIndex);
                    cmd.SetRayTracingBufferParam(m_Shader, s_PRNGStates, PRNGStates);
                    cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
                    cmd.SetRayTracingVectorParam(m_Shader, s_OutputTargetSize, outputTargetSize);
                    cmd.DispatchRays(m_Shader, "AntialiasingRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
                }

                context.ExecuteCommandBuffer(cmd);
                if (camera.cameraType == CameraType.Game)
                    m_FrameIndex++;
            }

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