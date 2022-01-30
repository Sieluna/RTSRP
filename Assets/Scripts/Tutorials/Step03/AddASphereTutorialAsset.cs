using UnityEngine;

[CreateAssetMenu(fileName = "AddASphereTutorialAsset", menuName = "Rendering/AddASphereTutorialAsset")]
public class AddASphereTutorialAsset : RayTracingTutorialAsset
{
    public override RayTracingTutorial CreateTutorial() => new AddASphereTutorial(this);
}