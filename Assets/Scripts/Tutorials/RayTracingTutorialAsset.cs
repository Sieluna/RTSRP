using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class RayTracingTutorialAsset : ScriptableObject
{
    public RayTracingShader Shader;

    public abstract RayTracingTutorial CreateTutorial();
}