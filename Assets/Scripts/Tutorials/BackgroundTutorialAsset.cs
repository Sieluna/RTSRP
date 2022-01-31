using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundTutorialAsset", menuName = "Rendering/BackgroundTutorialAsset")]
public class BackgroundTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial() => new BackgroundTutorial(this);
}
