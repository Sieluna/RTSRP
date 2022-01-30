using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class SceneManager : MonoBehaviour
{
    private const int k_MaxNumSubMeshes = 32;

    private static SceneManager s_Instance;

    public Renderer[] renderers;

    private readonly RayTracingSubMeshFlags[] m_SubMeshFlagArray = new RayTracingSubMeshFlags[k_MaxNumSubMeshes];

    public bool IsDirty { get; set; } = true;

    public static SceneManager Instance
    {
        get
        {
            if (s_Instance is not null) return s_Instance;

            s_Instance = FindObjectOfType<SceneManager>();
            s_Instance.Init();

            return s_Instance;
        }
    }

    public void Awake()
    {
        if (Application.isPlaying)
            DontDestroyOnLoad(this);

        IsDirty = true;
    }

    public void FillAccelerationStructure(ref RayTracingAccelerationStructure accelerationStructure)
    {
        foreach (var targetRenderer in renderers)
            if (targetRenderer)
                accelerationStructure.AddInstance(targetRenderer, m_SubMeshFlagArray);
    }

    private void Init()
    {
        for (var i = 0; i < k_MaxNumSubMeshes; ++i)
        {
            m_SubMeshFlagArray[i] = RayTracingSubMeshFlags.Enabled;
        }
    }
}