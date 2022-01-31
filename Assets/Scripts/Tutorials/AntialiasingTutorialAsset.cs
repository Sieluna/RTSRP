using UnityEngine;

[CreateAssetMenu(fileName = "AntialiasingTutorialAsset", menuName = "Rendering/AntialiasingTutorialAsset")]
public class AntialiasingTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial() => new AntialiasingTutorial(this);
}