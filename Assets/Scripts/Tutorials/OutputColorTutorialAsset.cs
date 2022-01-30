using UnityEngine;

[CreateAssetMenu(fileName = "OutputColorTutorialAsset", menuName = "Rendering/OutputColorTutorialAsset")]
public class OutputColorTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial() => new OutputColorTutorial(this);
}