using UnityEngine;
using UnityEngine.Rendering;

public class TutorialController : MonoBehaviour
{
    public RenderPipelineAsset renderPipelineAsset;

    public void Start()
    {
        GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
    }

    public void OnDestroy()
    {
        GraphicsSettings.renderPipelineAsset = null;
    }
}