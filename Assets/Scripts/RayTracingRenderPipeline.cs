using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RayTracingRenderPipeline : RenderPipeline
{
    public static readonly int s_AccelerationStructure = Shader.PropertyToID("_AccelerationStructure");

    private readonly RayTracingRenderPipelineAsset m_Asset;
    private readonly Dictionary<int, ComputeBuffer> m_PRNGStates = new();
    private RayTracingAccelerationStructure m_AccelerationStructure;
    private RayTracingTutorial m_Tutorial;

    public RayTracingAccelerationStructure AccelerationStructure => m_AccelerationStructure;

    public RayTracingRenderPipeline(RayTracingRenderPipelineAsset asset)
    {
        m_Asset = asset;
        m_AccelerationStructure = new RayTracingAccelerationStructure();

        m_Tutorial = m_Asset.TutorialAsset.CreateTutorial();
        if (m_Tutorial is null)
        {
            Debug.LogError("Can't create tutorial.");
            return;
        }

        if (m_Tutorial.Init(this) == false)
        {
            m_Tutorial = null;
            Debug.LogError("Initialize tutorial failed.");
        }
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        if (!SystemInfo.supportsRayTracing)
        {
            Debug.LogError("You system is not support ray tracing. Please check your graphic API is D3D12.");
            return;
        }

        BeginFrameRendering(context, cameras);

        Array.Sort(cameras, (lhs, rhs) => (int) (lhs.depth - rhs.depth));

        BuildAccelerationStructure();

        foreach (var camera in cameras)
        {
            if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
                continue;

            BeginCameraRendering(context, camera);
            m_Tutorial?.Render(context, camera);
            context.Submit();
            EndCameraRendering(context, camera);
        }

        EndFrameRendering(context, cameras);
    }

    protected override void Dispose(bool disposing)
    {
        m_Tutorial?.Dispose(disposing);
        m_Tutorial = null;

        foreach (var pair in m_PRNGStates) pair.Value.Release();

        m_PRNGStates.Clear();

        m_AccelerationStructure?.Dispose();
        m_AccelerationStructure = null;
    }

    private void BuildAccelerationStructure()
    {
        if (SceneManager.Instance is null || !SceneManager.Instance.IsDirty) return;

        m_AccelerationStructure?.Dispose();
        m_AccelerationStructure = new RayTracingAccelerationStructure();

        SceneManager.Instance.FillAccelerationStructure(ref m_AccelerationStructure);

        m_AccelerationStructure.Build();

        SceneManager.Instance.IsDirty = false;
    }
}