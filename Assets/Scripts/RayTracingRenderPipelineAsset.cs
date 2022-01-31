using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "RayTracingRenderPipelineAsset", menuName = "Rendering/RayTracingRenderPipelineAsset", order = -1)]
public class RayTracingRenderPipelineAsset : RenderPipelineAsset
{
    public RayTracingTutorialAsset TutorialAsset;

    protected override RenderPipeline CreatePipeline() => new RayTracingRenderPipeline(this);
}