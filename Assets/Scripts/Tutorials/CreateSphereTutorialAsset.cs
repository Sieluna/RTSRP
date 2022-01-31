using UnityEngine;

[CreateAssetMenu(fileName = "CreateSphereTutorialAsset", menuName = "Rendering/CreateSphereTutorialAsset")]
public class CreateSphereTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial() => new CreateSphereTutorial(this);
}