using UnityEngine;
using UnityEngine.Rendering;

public class CameraTutorial : RayTracingTutorial
{
    private static readonly int s_FrameIndex = Shader.PropertyToID("_FrameIndex");
    private static readonly int s_PRNGStates = Shader.PropertyToID("_PRNGStates");
    private static readonly int s_FocusCameraLeftBottomCorner = Shader.PropertyToID("_FocusCameraLeftBottomCorner");
    private static readonly int s_FocusCameraRight = Shader.PropertyToID("_FocusCameraRight");
    private static readonly int s_FocusCameraUp = Shader.PropertyToID("_FocusCameraUp");
    private static readonly int s_FocusCameraSize = Shader.PropertyToID("_FocusCameraSize");
    private static readonly int s_FocusCameraHalfAperture = Shader.PropertyToID("_FocusCameraHalfAperture");

    private int m_FrameIndex;

    public CameraTutorial(RayTracingTutorialAsset asset) : base(asset) { }

    public override void Render(ScriptableRenderContext context, Camera camera)
    {
        base.Render(context, camera);
        var focusCamera = camera.GetComponent<FocusCamera>();
        if (focusCamera is null) return;

        var outputTarget = RequireOutputTarget(camera);
        var outputTargetSize = RequireOutputTargetSize(camera);

        var accelerationStructure = m_Pipeline.RequestAccelerationStructure();
        var PRNGStates = m_Pipeline.RequirePRNGStates(camera);

        var cmd = CommandBufferPool.Get(nameof(CameraTutorial));
        try
        {
            if (m_FrameIndex < 1000)
            {
                using (new ProfilingScope(cmd, new ProfilingSampler("RayTracing")))
                {
                    cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraLeftBottomCorner, focusCamera.LeftBottomCorner);
                    cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraRight, focusCamera.transform.right);
                    cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraUp, focusCamera.transform.up);
                    cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraSize, focusCamera.Size);
                    cmd.SetRayTracingFloatParam(m_Shader, s_FocusCameraHalfAperture, focusCamera.Aperture * 0.5f);

                    cmd.SetRayTracingShaderPass(m_Shader, "RayTracing");
                    cmd.SetRayTracingAccelerationStructure(m_Shader, RayTracingRenderPipeline.s_AccelerationStructure, accelerationStructure);
                    cmd.SetRayTracingIntParam(m_Shader, s_FrameIndex, m_FrameIndex);
                    cmd.SetRayTracingBufferParam(m_Shader, s_PRNGStates, PRNGStates);
                    cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
                    cmd.SetRayTracingVectorParam(m_Shader, s_OutputTargetSize, outputTargetSize);
                    cmd.DispatchRays(m_Shader, "CameraRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
                }

                context.ExecuteCommandBuffer(cmd);

                if (camera.cameraType == CameraType.Game) m_FrameIndex++;
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