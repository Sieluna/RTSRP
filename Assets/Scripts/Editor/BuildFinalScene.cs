using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BuildFinalScene
{
    private static MersenneTwister s_MT = new();
    
    [MenuItem("Tutorial/Generate Final Scene")]
    public static void GenerateScene()
    {
        s_MT.InitGenRand(95273);

        var templateGo = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Sphere.prefab");
        var templateLambertian = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RandomLambertian.mat");
        var templateMetal = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RandomMetal.mat");
        var templateDielectric = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/RandomDielectric.mat");

        var renderers = new List<Renderer>();

        for (var a = -11; a < 11; ++a)
        {
            for (var b = -11; b < 11; ++b)
            {
                var center = new Vector3(a + 0.9f * (float)s_MT.GenRandReal1(), 0.2f, b + 0.9f * (float)s_MT.GenRandReal1());
                if ((center - new Vector3(4.0f, 0.2f, 0.0f)).magnitude > 0.9f)
                {
                    renderers.Add(CreateSphere(templateGo, templateLambertian, templateMetal, templateDielectric, center, a, b));
                }
            }
        }

        renderers.Add(CreateLargeSphere(templateGo, "Sphere_D", new Vector3(0.0f, 1.0f, 0.0f), Vector3.one * 2.0f, templateDielectric, Color.white, 1.5f));
        renderers.Add(CreateLargeSphere(templateGo, "Sphere_L", new Vector3(-4.0f, 1.0f, 0.0f), Vector3.one * 2.0f, templateLambertian, new Color(0.4f, 0.2f, 0.1f)));
        renderers.Add(CreateLargeSphere(templateGo, "Sphere_M", new Vector3(4.0f, 1.0f, 0.0f), Vector3.one * 2.0f, templateMetal, new Color(0.7f, 0.6f, 0.5f)));

        SceneManager.Instance.renderers = renderers.ToArray();
    }

    private static Renderer CreateSphere(GameObject templateGo, Material templateLambertian, Material templateMetal, Material templateDielectric, Vector3 center, int a, int b)
    {
        var chooseMat = s_MT.GenRandReal1();
        Renderer renderer;
        Material material;

        var go = Object.Instantiate(templateGo);

        if (chooseMat < 0.8)
        {
            go.name = $"Sphere_L_{a}_{b}";
            renderer = SetupRenderer(go, center, Vector3.one * 0.4f);
            material = new Material(templateLambertian);
            material.SetColor("_Color", new Color(
                (float)(s_MT.GenRandReal1() * s_MT.GenRandReal1()),
                (float)(s_MT.GenRandReal1() * s_MT.GenRandReal1()),
                (float)(s_MT.GenRandReal1() * s_MT.GenRandReal1())));
        }
        else if (chooseMat < 0.95)
        {
            go.name = $"Sphere_M_{a}_{b}";
            renderer = SetupRenderer(go, center, Vector3.one * 0.4f);
            material = new Material(templateMetal);
            material.SetColor("_Color", new Color(
                0.5f * (1.0f + (float)s_MT.GenRandReal1()),
                0.5f * (1.0f + (float)s_MT.GenRandReal1()),
                0.5f * (1.0f + (float)s_MT.GenRandReal1())));
            material.SetFloat("_Fuzz", 0.5f * (float)s_MT.GenRandReal1());
        }
        else
        {
            go.name = $"Sphere_D_{a}_{b}";
            renderer = SetupRenderer(go, center, Vector3.one * 0.4f);
            material = new Material(templateDielectric);
            material.SetColor("_Color", Color.white);
            material.SetFloat("_IOR", 1.5f);
        }

        renderer.material = material;

        return renderer;
    }

    private static Renderer CreateLargeSphere(GameObject templateGo, string name, Vector3 position, Vector3 scale, Material material, Color color, float ior = 0)
    {
        var go = Object.Instantiate(templateGo);
        go.name = name;
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        
        var renderer = go.GetComponent<Renderer>();
        var newMaterial = new Material(material);
        newMaterial.SetColor("_Color", color);
        if (ior > 0)
        {
            newMaterial.SetFloat("_IOR", ior);
        }
        renderer.material = newMaterial;
        
        return renderer;
    }

    private static Renderer SetupRenderer(GameObject go, Vector3 position, Vector3 scale)
    {
        go.transform.localPosition = position;
        go.transform.localScale = scale;
        return go.GetComponent<Renderer>();
    }
}
