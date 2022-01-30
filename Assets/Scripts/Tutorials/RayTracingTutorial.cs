using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public abstract class RayTracingTutorial
{
    protected static readonly int s_OutputTarget = Shader.PropertyToID("_OutputTarget");
    protected static readonly int s_OutputTargetSize = Shader.PropertyToID("_OutputTargetSize");
    private static readonly int s_WorldSpaceCameraPos = Shader.PropertyToID("_WorldSpaceCameraPos");
    private static readonly int s_InvCameraViewProj = Shader.PropertyToID("_InvCameraViewProj");
    private static readonly int s_CameraFarDistance = Shader.PropertyToID("_CameraFarDistance");

    private readonly RayTracingTutorialAsset m_Asset;

    private readonly Dictionary<int, RTHandle> m_OutputTargets = new();
    private readonly Dictionary<int, Vector4> m_OutputTargetSizes = new();

    protected RayTracingRenderPipeline m_Pipeline;
    protected RayTracingShader m_Shader;

    protected RayTracingTutorial(RayTracingTutorialAsset asset) => m_Asset = asset;

    public virtual bool Init(RayTracingRenderPipeline pipeline)
    {
        m_Pipeline = pipeline;
        m_Shader = m_Asset.Shader;
        return true;
    }

    public virtual void Render(ScriptableRenderContext context, Camera camera)
    {
        Shader.SetGlobalVector(s_WorldSpaceCameraPos, camera.transform.position);
        var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
        var viewMatrix = camera.worldToCameraMatrix;
        var viewProjMatrix = projMatrix * viewMatrix;
        var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
        Shader.SetGlobalMatrix(s_InvCameraViewProj, invViewProjMatrix);
        Shader.SetGlobalFloat(s_CameraFarDistance, camera.farClipPlane);
    }

    public virtual void Dispose(bool disposing)
    {
        foreach (var pair in m_OutputTargets) RTHandles.Release(pair.Value);
        m_OutputTargets.Clear();
    }

    protected RTHandle RequireOutputTarget(Camera camera)
    {
        var id = camera.GetInstanceID();

        if (m_OutputTargets.TryGetValue(id, out var outputTarget))
            return outputTarget;

        outputTarget = RTHandles.Alloc(camera.pixelWidth,
            camera.pixelHeight,
            colorFormat: GraphicsFormat.R32G32B32A32_SFloat,
            wrapMode: TextureWrapMode.Clamp,
            enableRandomWrite: true,
            autoGenerateMips: false,
            name: $"OutputTarget_{camera.name}");

        m_OutputTargets.Add(id, outputTarget);

        return outputTarget;
    }

    protected Vector4 RequireOutputTargetSize(Camera camera)
    {
        var id = camera.GetInstanceID();

        if (m_OutputTargetSizes.TryGetValue(id, out var outputTargetSize))
            return outputTargetSize;

        outputTargetSize = new Vector4(camera.pixelWidth, camera.pixelHeight, 1.0f / camera.pixelWidth, 1.0f / camera.pixelHeight);
        m_OutputTargetSizes.Add(id, outputTargetSize);

        return outputTargetSize;
    }
}