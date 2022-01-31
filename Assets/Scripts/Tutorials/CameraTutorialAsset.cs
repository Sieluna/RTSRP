using UnityEngine;

[CreateAssetMenu(fileName = "CameraTutorialAsset", menuName = "Rendering/CameraTutorialAsset")]
public class CameraTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial() => new CameraTutorial(this);
}