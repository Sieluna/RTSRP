<div align="center">

# Ray Tracing SRP

Unity RTX On!

</div>

## Table of Content:

- [GPU Ray Tracing](#)
  - [Overview](#overview)
    - [Setting Up the Environment](#setting-up-the-environment)
    - [Import the Dependencies](#import-the-dependencies)
  - [1. Outputting an Image](#1-outputting-an-image)
    - [1.1. Create a RayTraceShader in Unity](#11-create-a-raytraceshader-in-unity)
    - [1.2. Rendering in C# using the SRP Pipeline](#12-rendering-in-c-using-the-srp-pipeline)
    - [1.3. Final Output](#13-final-output)
  - [2. Outputting the Background](#2-outputting-the-background)
    - [2.1. Create a RayTraceShader in Unity](#21-creating-a-raytraceshader-in-unity)
    - [2.2. Rendering in C# using the SRP Pipeline](#22-rendering-in-c-using-the-srp-pipeline)
    - [2.3. Final Output](#23-final-output)
  - [3. Rendering a Sphere](#3-rendering-a-sphere)
    - [3.1. Create a RayTraceShader in Unity](#31-creating-a-raytraceshader-in-unity)
    - [3.2. Creating the Sphere Shader](#32-creating-the-sphere-shader)
    - [3.3. Rendering in C# using the SRP Pipeline](#33-rendering-in-c-using-the-srp-pipeline)
    - [3.4. Final Output](#34-final-output)
  - [4. Rendering the normal](#4-rendering-the-normal)
    - [4.1. Creating the Sphere Shader](#41-creating-the-sphere-shader)
    - [4.2. Final Output](#42-final-output)
  - [5. Antialiasing](#5-antialiasing)
    - [5.1. Create a RayTraceShader in Unity](#51-create-a-raytraceshader-in-unity)
    - [5.2. Rendering in C# using the SRP Pipeline](#52-rendering-in-c-using-the-srp-pipeline)
    - [5.3. Final Output](#53-final-output)
  - [6. Diffuse Material](#6-diffuse-material)
    - [6.1. Create a RayTraceShader in Unity](#61-create-a-raytraceshader-in-unity)
    - [6.2. Creating the Object Shader](#62-creating-the-object-shader)
    - [6.3. Final Output](#63-final-output)
  - [7. Dielectric Material](#7-dielectric-material)
    - [7.1. Object ClosestHitShader](#71-object-closesthitshader)
    - [7.2. Final Output](#72-final-output)
  - [8. Depth of Field Blur](#8-depth-of-field-blur)
    - [8.1. C# Code](#81-c-code)
    - [8.2. RayTrace Shader](#82-raytrace-shader)
    - [8.3. Final Output](#83-final-output)
  - [9. Bringing It All Together](#9-bringing-it-all-together)

## Overview

This tutorial is based on [Ray Tracing in One Weekend](https://raytracing.github.io/books/RayTracingInOneWeekend.html)
and explains how to implement Ray Tracing using Unity's SRP and DXR. Therefore,
before reading this article, it’s recommended to first go through the tutorial.
The core algorithms are covered there, and this document focuses on translating
those concepts into Unity's environment.

### Setting Up the Environment

This implementation is based on Unity 2020+ with integrated DXR, requiring basic
configuration for the Unity project.

![Setup DX12](Images/0_SetupDX12.png)

The first step is to configure Unity to use Direct3D12 by going to **Project
Settings → Player → Other Settings**, and setting the **Graphics API** to
**Direct3D12**.

### Import the Dependencies

Since we are using the SRP (Scriptable Render Pipeline) library, the following
Unity packages must be imported:

![Import SRP](Images/0_ImportPackage.png)

## 1. Outputting an Image

**Tutorial class**: `OutputColorTutorial`

**Scene file**: `OutputColorTutorialScene`

A common starting point in ray tracing is to output an image based on pixel
coordinates. This simple exercise allows us to explore the capabilities of ray
tracing.

### 1.1. Create a RayTraceShader in Unity

In this step, we create a simple RayTraceShader in Unity. This shader will
calculate pixel values based on their coordinates and assign colors accordingly.
To begin, follow these steps:

1. Right-click in the **Project** panel and select **Create → Shader → Ray
   Tracing Shader**.

   ![Create RayTraceShader](Images/1_OutputImage1.png)

2. Open the created shader and replace its content with the following code:

```glsl
#pragma max_recursion_depth 1

RWTexture2D<float4> _OutputTarget;

[shader("raygeneration")]
void OutputColorRayGenShader()
{
  uint2 dispatchIdx = DispatchRaysIndex().xy;
  uint2 dispatchDim = DispatchRaysDimensions().xy;

  _OutputTarget[dispatchIdx] = float4((float)dispatchIdx.x / dispatchDim.x, (float)dispatchIdx.y / dispatchDim.y, 0.2f, 1.0f);
}
```

This shader will generate an image where the red (R) and green (G) channels
change smoothly from 0 to 1 based on the pixel's horizontal and vertical
position, respectively. The blue (B) channel remains constant across all pixels.

* **DispatchRaysIndex().xy** returns the current pixel position.

* **DispatchRaysDimensions().xy** returns the dimensions of the render target.

### 1.2. Rendering in C# using the SRP Pipeline

The following C# code integrates the shader into Unity's render pipeline:

```csharp
var outputTarget = RequireOutputTarget(camera);

var cmd = CommandBufferPool.Get(nameof(OutputColorTutorial));
try
{
  using (new ProfilingSample(cmd, "RayTracing"))
  {
    cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
    cmd.DispatchRays(m_Shader, "OutputColorRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
  }
  context.ExecuteCommandBuffer(cmd);

  using (new ProfilingSample(cmd, "FinalBlit"))
  {
    cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
  }
  context.ExecuteCommandBuffer(cmd);
}
finally
{
  CommandBufferPool.Release(cmd);
}
```

Since the Ray Trace Shader only renders to a render target, a **Blit** operation
is required to display the result on the screen.

* **RequireOutputTarget** retrieves the render target based on the camera's
  output.

* **cmd.SetRayTracingTextureParam** sets the render target for the Ray Trace
  Shader. Here, *m_Shader* refers to the Ray Tracing Shader Program, and
  *s_OutputTarget* is obtained via `Shader.PropertyToID("_OutputTarget")`.

* **cmd.DispatchRays** dispatches the ray generation function `OutputColorRayGenShader`
  in the RayTrace Shader for Ray Tracing.

### 1.3. Final Output

After setting everything up, running the code should produce the following image:

![Final Output](Images/1_OutputImage2.png)

## 2. Outputting the Background

**Tutorial class**: BackgroundTutorial

**Scene file**: BackgroundTutorialScene

This section builds on the previous one by rendering a gradient background based
on the direction of the rays. Instead of outputting simple coordinates, we will
calculate the ray's direction and use that to generate a gradient color from the
top to the bottom of the image.

### 2.1. Creating a RayTraceShader in Unity

Create the following Ray Tracing Shader to generate the background:

```glsl
inline void GenerateCameraRay(out float3 origin, out float3 direction)
{
  // center in the middle of the pixel.
  float2 xy = DispatchRaysIndex().xy + 0.5f;
  float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

  // Un project the pixel coordinate into a ray.
  float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));

  world.xyz /= world.w;
  origin = _WorldSpaceCameraPos.xyz;
  direction = normalize(world.xyz - origin);
}

inline float3 Color(float3 origin, float3 direction)
{
  float t = 0.5f * (direction.y + 1.0f);
  return (1.0f - t) * float3(1.0f, 1.0f, 1.0f) + t * float3(0.5f, 0.7f, 1.0f);
}

[shader("raygeneration")]
void BackgroundRayGenShader()
{
  const uint2 dispatchIdx = DispatchRaysIndex().xy;

  float3 origin;
  float3 direction;
  GenerateCameraRay(origin, direction);

  _OutputTarget[dispatchIdx] = float4(Color(origin, direction), 1.0f);
}
```

* **GenerateCameraRay** calculates the ray's origin and direction based on the
  pixel's position in the camera's viewport.

* The **Color** function calculates a gradient from top to bottom, creating the
  background effect.

### 2.2. Rendering in C# using the SRP Pipeline

To render this background, we need to pass the camera's parameters to the
RayTraceShader. This includes the camera's world-space position and the inverse
view-projection matrix, which is used to transform the screen-space coordinates
into world-space rays.

```csharp
Shader.SetGlobalVector(CameraShaderParams._WorldSpaceCameraPos, camera.transform.position);
var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
var viewMatrix = camera.worldToCameraMatrix;
var viewProjMatrix = projMatrix * viewMatrix;
var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
Shader.SetGlobalMatrix(CameraShaderParams._InvCameraViewProj, invViewProjMatrix);
```

Once these parameters are set, we can dispatch the rays to generate the
background image. Just like before, we use a **Blit** operation to display the
final output on the screen.


```csharp
var outputTarget = RequireOutputTarget(camera);

var cmd = CommandBufferPool.Get(nameof(BackgroundTutorial));
try
{
  using (new ProfilingSample(cmd, "RayTracing"))
  {
    cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
    cmd.DispatchRays(m_Shader, "BackgroundRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
  }
  context.ExecuteCommandBuffer(cmd);

  using (new ProfilingSample(cmd, "FinalBlit"))
  {
    cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
  }
  context.ExecuteCommandBuffer(cmd);
}
finally
{
  CommandBufferPool.Release(cmd);
}
```

### 2.3. Final Output

The resulting image should appear as follows:

![Background](Images/2_Background1.png)

## 3. Rendering a Sphere

**Tutorial Class**: CreateSphereTutorial

**Scene File**: CreateTutorialScene

In this section, we extend the previous example by rendering a 3D sphere.
Normally, in ray tracing, spheres can be represented mathematically using
intersection shaders. However, due to Unity's lack of support for procedural
geometry in DXR, we will use a pre-modeled sphere mesh instead.

### 3.1. Creating a RayTraceShader in Unity

```glsl
struct RayIntersection
{
  float4 color;
};

inline float3 BackgroundColor(float3 origin, float3 direction)
{
  float t = 0.5f * (direction.y + 1.0f);
  return (1.0f - t) * float3(1.0f, 1.0f, 1.0f) + t * float3(0.5f, 0.7f, 1.0f);
}

[shader("raygeneration")]
void AddASphereRayGenShader()
{
  const uint2 dispatchIdx = DispatchRaysIndex().xy;

  float3 origin;
  float3 direction;
  GenerateCameraRay(origin, direction);

  RayDesc rayDescriptor;
  rayDescriptor.Origin = origin;
  rayDescriptor.Direction = direction;
  rayDescriptor.TMin = 1e-5f;
  rayDescriptor.TMax = _CameraFarDistance;

  RayIntersection rayIntersection;
  rayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);

  TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, rayIntersection);

  _OutputTarget[dispatchIdx] = rayIntersection.color;
}

[shader("miss")]
void MissShader(inout RayIntersection rayIntersection : SV_RayPayload)
{
  float3 origin = WorldRayOrigin();
  float3 direction = WorldRayDirection();
  rayIntersection.color = float4(BackgroundColor(origin, direction), 1.0f);
}
```

In the **ray generation shader**, the `TraceRay` function is used to cast rays.
For detailed usage, please refer to the
[Microsoft DXR documentation](https://microsoft.github.io/DirectX-Specs/d3d/Raytracing.html#hit-groups).

* **RayDesc** describes the ray, with *Origin* as the starting point of the ray,
  *Direction* as the ray’s direction, *TMin* as the minimum value of t, and
  *TMax* as the maximum value of t (in the code, it is set to the camera's far
  clipping distance).
* **RayIntersection** is a user-defined Ray Payload structure that is used to
  pass data during ray tracing. In this case, the *color* field is used to store
  the result of the ray trace.

Additionally, we have included a **miss shader**, which is executed when no ray
intersections are detected. In this shader, we call the *BackgroundColor*
function to return the gradient background color defined in section 3.

### 3.2. Creating the Sphere Shader

Create a surface shader file in Unity and add the following code at the end.

```glsl
SubShader
{
  Pass
  {
    Name "RayTracing"
    Tags { "LightMode" = "RayTracing" }

    HLSLPROGRAM

    #pragma raytracing test

    struct RayIntersection
    {
      float4 color;
    };

    CBUFFER_START(UnityPerMaterial)
    float4 _Color;
    CBUFFER_END

    [shader("closesthit")]
    void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
    {
      rayIntersection.color = _Color;
    }

    ENDHLSL
  }
}
```

In this shader, we define a **SubShader** with a pass named "RayTracing." This
name can be customized as needed. In the subsequent C# code, you will see how
this pass is specifically referenced.

The directive **#pragma raytracing test** indicates that this is a Ray Tracing
pass.

The **ClosestHitShader** accepts two parameters:

1. **rayIntersection**, which carries the Ray Payload data passed from the ray
   generation shader.
2. **attributeData**, which contains intersection data, although it is not used
   in this example. The shader simply returns a predefined color.

### 3.3. Rendering in C# using the SRP Pipeline

Setting the camera parameters in C# follows the same approach as in section 3.

The following C# code is used in the SRP pipeline:

```csharp
var outputTarget = RequireOutputTarget(camera);

var accelerationStructure = m_Pipeline.RequestAccelerationStructure();

var cmd = CommandBufferPool.Get(nameof(CreateSphereTutorial));
try
{
  using (new ProfilingSample(cmd, "RayTracing"))
  {
    cmd.SetRayTracingShaderPass(m_Shader, "RayTracing");
    cmd.SetRayTracingAccelerationStructure(m_Shader, RayTracingRenderPipeline.s_AccelerationStructure, accelerationStructure);
    cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
    cmd.DispatchRays(m_Shader, "CreateSphereRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
  }
  context.ExecuteCommandBuffer(cmd);

  using (new ProfilingSample(cmd, "FinalBlit"))
  {
    cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
  }
  context.ExecuteCommandBuffer(cmd);
}
finally
{
  CommandBufferPool.Release(cmd);
}
```

### 3.4. Final Output

The resulting image should appear as follows:

![Sphere](Images/3_AddASphere1.png)

## 4. Rendering the normal

**Tutorial Class**: CreateSphereTutorial

**Scene File**: SurfaceNormalTutorialScene

This section is a modification of the routine from section 4, with the only
change being the shader for the object itself.

### 4.1. Creating the Sphere Shader

```glsl
struct RayIntersection
{
  float4 color;
};

struct IntersectionVertex
{
  // Object space normal of the vertex
  float3 normalOS;
};

void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
{
  outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
}

[shader("closesthit")]
void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
{
  // Fetch the indices of the currentr triangle
  uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

  // Fetch the 3 vertices
  IntersectionVertex v0, v1, v2;
  FetchIntersectionVertex(triangleIndices.x, v0);
  FetchIntersectionVertex(triangleIndices.y, v1);
  FetchIntersectionVertex(triangleIndices.z, v2);

  // Compute the full barycentric coordinates
  float3 barycentricCoordinates = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);

  float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
  float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
  float3 normalWS = normalize(mul(objectToWorld, normalOS));

  rayIntersection.color = float4(0.5f * (normalWS + 1.0f), 0);
}
```

* **UnityRayTracingFetchTriangleIndices** is a Unity utility function used to
  obtain the indices of the triangle that the ray intersects, based on the index
  information returned by *PrimitiveIndex*.
* The **IntersectionVertex** structure defines the vertex information of the
  intersected triangle that we are interested in during ray tracing
* The **FetchIntersectionVertex** function populates the *IntersectionVertex*
  data by internally calling the *UnityRayTracingFetchVertexAttribute3* function,
  a Unity utility function, to retrieve the vertex data. In this case, the
  Object Space normal of the vertex is obtained.
* **INTERPOLATE_RAYTRACING_ATTRIBUTE** is used to interpolate between the three
  vertices of the intersected triangle to compute the intersection point’s data.

### 4.2. Final Output

![Normal Sphere](Images/4_NormalAsColor1.png)

## 5. Antialiasing

**Tutorial Class**: AntialiasingTutorial

**Scene File**: AntialiasingTutorialScene

When zooming in on the final output image from section 4, you can observe
significant aliasing issues.

![Before Antialiasing](Images/5_Antialiasing1.png)

In this example, the Accumulate Average Sample method is used instead.

### 5.1. Create a RayTraceShader in Unity

```glsl
struct RayIntersection
{
  uint4 PRNGStates;
  float4 color;
};

inline void GenerateCameraRayWithOffset(out float3 origin, out float3 direction, float2 offset)
{
  float2 xy = DispatchRaysIndex().xy + offset;
  float2 screenPos = xy / DispatchRaysDimensions().xy * 2.0f - 1.0f;

  // Un project the pixel coordinate into a ray.
  float4 world = mul(_InvCameraViewProj, float4(screenPos, 0, 1));

  world.xyz /= world.w;
  origin = _WorldSpaceCameraPos.xyz;
  direction = normalize(world.xyz - origin);
}

[shader("raygeneration")]
void AntialiasingRayGenShader()
{
  const uint2 dispatchIdx = DispatchRaysIndex().xy;
  const uint PRNGIndex = dispatchIdx.y * (int)_OutputTargetSize.x + dispatchIdx.x;
  uint4 PRNGStates = _PRNGStates[PRNGIndex];

  float4 finalColor = float4(0, 0, 0, 0);
  {
    float3 origin;
    float3 direction;
    float2 offset = float2(GetRandomValue(PRNGStates), GetRandomValue(PRNGStates));
    GenerateCameraRayWithOffset(origin, direction, offset);

    RayDesc rayDescriptor;
    rayDescriptor.Origin = origin;
    rayDescriptor.Direction = direction;
    rayDescriptor.TMin = 1e-5f;
    rayDescriptor.TMax = _CameraFarDistance;

    RayIntersection rayIntersection;
    rayIntersection.PRNGStates = PRNGStates;
    rayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, rayIntersection);
    PRNGStates = rayIntersection.PRNGStates;
    finalColor += rayIntersection.color;
  }

  _PRNGStates[PRNGIndex] = PRNGStates;
  if (_FrameIndex > 1)
  {
    float a = 1.0f / (float)_FrameIndex;
    finalColor = _OutputTarget[dispatchIdx] * (1.0f - a) + finalColor * a;
  }

  _OutputTarget[dispatchIdx] = finalColor;
}
```

The GenerateCameraRayWithOffset function applies an offset to the ray generated
for the pixel, with the offset value provided by GetRandomValue.

The _FrameIndex represents the index of the current frame being rendered. If
this value is greater than 1, the current frame data is averaged with previous
frames; otherwise, the current frame data is directly written to the render
target.

### 5.2. Rendering in C# using the SRP Pipeline

```csharp
var outputTarget = RequireOutputTarget(camera);
var outputTargetSize = RequireOutputTargetSize(camera);

var accelerationStructure = m_Pipeline.RequestAccelerationStructure();
var PRNGStates = m_Pipeline.RequirePRNGStates(camera);

var cmd = CommandBufferPool.Get(typeof(OutputColorTutorial).Name);
try
{
  if (m_FrameIndex < 1000)
  {
    using (new ProfilingSample(cmd, "RayTracing"))
    {
      cmd.SetRayTracingShaderPass(m_Shader, "RayTracing");
      cmd.SetRayTracingAccelerationStructure(m_Shader, RayTracingRenderPipeline.s_AccelerationStructure, accelerationStructure);
      cmd.SetRayTracingIntParam(m_Shader, s_FrameIndex, m_FrameIndex);
      cmd.SetRayTracingBufferParam(m_Shader, s_PRNGStates, PRNGStates);
      cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
      cmd.SetRayTracingVectorParam(m_Shader, s_OutputTargetSize, outputTargetSize);
      cmd.DispatchRays(m_Shader, "AntialiasingRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
    }
    context.ExecuteCommandBuffer(cmd);
    if (camera.cameraType == CameraType.Game)
      m_FrameIndex++;
  }

  using (new ProfilingSample(cmd, "FinalBlit"))
  {
    cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
  }
  context.ExecuteCommandBuffer(cmd);
}
finally
{
  CommandBufferPool.Release(cmd);
}
```

The **RequireOutputTargetSize** function retrieves the current render target's
size.

The **RequirePRNGStates** function retrieves the buffer for the random number
generator state.

The **_frameIndex** indicates the index of the current frame being rendered.
Rendering stops after accumulating 1000 frames.

### 5.3. Final Output

![After Antialiasing](Images/5_Antialiasing2.png)

## 6. Diffuse Material

**Tutorial Class**: AntialiasingTutorial

**Scene File**: DiffuseTutorialScene

For the implementation of Diffuse, refer to the original text.

### 6.1. Create a RayTraceShader in Unity

The code is mostly the same as the previous section, with the addition of the
remainingDepth field in the Ray Payload to track how many recursions remain.
The maximum recursion count is set by MAX_DEPTH. The value of MAX_DEPTH must be
less than max_recursion_depth minus 1.

```glsl
RayIntersection rayIntersection;
rayIntersection.remainingDepth = MAX_DEPTH - 1;
rayIntersection.PRNGStates = PRNGStates;
rayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);

TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, rayIntersection);
PRNGStates = rayIntersection.PRNGStates;
finalColor += rayIntersection.color;
```

### 6.2. Creating the Object Shader

Here is the code for the **ClosestHitShader**:

```glsl
[shader("closesthit")]
void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
{
  // Fetch the indices of the currentr triangle
  // Fetch the 3 vertices
  // Compute the full barycentric coordinates
  // Get normal in world space.
  ...
  float3 normalWS = normalize(mul(objectToWorld, normalOS));

  float4 color = float4(0, 0, 0, 1);
  if (rayIntersection.remainingDepth > 0)
  {
    // Get position in world space.
    float3 origin = WorldRayOrigin();
    float3 direction = WorldRayDirection();
    float t = RayTCurrent();
    float3 positionWS = origin + direction * t;

    // Make reflection ray.
    RayDesc rayDescriptor;
    rayDescriptor.Origin = positionWS + 0.001f * normalWS;
    rayDescriptor.Direction = normalize(normalWS + GetRandomOnUnitSphere(rayIntersection.PRNGStates));
    rayDescriptor.TMin = 1e-5f;
    rayDescriptor.TMax = _CameraFarDistance;

    // Tracing reflection.
    RayIntersection reflectionRayIntersection;
    reflectionRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
    reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
    reflectionRayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);

    rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
    color = reflectionRayIntersection.color;
  }

  rayIntersection.color = _Color * 0.5f * color;
}
```

The method for calculating normalWS (world-space normal) is the same as in the
previous section and is omitted here.

If rayIntersection.remainingDepth is greater than 0, the Diffuse calculation is
performed using the method from the original text, and TraceRay is called again
for recursive ray tracing.

The GetRandomOnUnitSphere function returns a randomly distributed vector on the
unit sphere.

### 6.3. Final Output

![Diffuse](Images/6_Diffuse1.png)

## 7. Dielectric Material

**Tutorial Class**: AntialiasingTutorial

**Scene File**: DielectricsTutorialScene

In the original text, a method with a negative radius is used to achieve a glass
bubble effect. Since Unity doesn’t allow the use of an Intersection Shader, a
new object shader called DielectricsInv was added to reverse the Normal in order
to achieve the same effect.

### 7.1. Object ClosestHitShader

```glsl
inline float schlick(float cosine, float IOR)
{
  float r0 = (1.0f - IOR) / (1.0f + IOR);
  r0 = r0 * r0;
  return r0 + (1.0f - r0) * pow((1.0f - cosine), 5.0f);
}

[shader("closesthit")]
void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
{
  // Fetch the indices of the currentr triangle
  // Fetch the 3 vertices
  // Compute the full barycentric coordinates
  // Get normal in world space.
  ...
  float3 normalWS = normalize(mul(objectToWorld, normalOS));

  float4 color = float4(0, 0, 0, 1);
  if (rayIntersection.remainingDepth > 0)
  {
    // Get position in world space.
    ...
    float3 positionWS = origin + direction * t;

    // Make reflection & refraction ray.
    float3 outwardNormal;
    float niOverNt;
    float reflectProb;
    float cosine;
    if (dot(-direction, normalWS) > 0.0f)
    {
      outwardNormal = normalWS;
      niOverNt = 1.0f / _IOR;
      cosine = _IOR * dot(-direction, normalWS);
    }
    else
    {
      outwardNormal = -normalWS;
      niOverNt = _IOR;
      cosine = -dot(-direction, normalWS);
    }
    reflectProb = schlick(cosine, _IOR);

    float3 scatteredDir;
    if (GetRandomValue(rayIntersection.PRNGStates) < reflectProb)
      scatteredDir = reflect(direction, normalWS);
    else
      scatteredDir = refract(direction, outwardNormal, niOverNt);

    RayDesc rayDescriptor;
    rayDescriptor.Origin = positionWS + 1e-5f * scatteredDir;
    rayDescriptor.Direction = scatteredDir;
    rayDescriptor.TMin = 1e-5f;
    rayDescriptor.TMax = _CameraFarDistance;

    // Tracing reflection or refraction.
    RayIntersection reflectionRayIntersection;
    reflectionRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
    reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
    reflectionRayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    TraceRay(_AccelerationStructure, RAY_FLAG_NONE, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);

    rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
    color = reflectionRayIntersection.color;
  }

  rayIntersection.color = _Color * color;
}
```

**_IOR** represents the material’s index of refraction.

The main difference between this and the Diffuse material is in the calculation
of reflection and refraction rays. For details on these algorithms, refer to the
original text.

In this case, the second argument of TraceRay is changed to RAY_FLAG_NONE, since
the refracted ray needs to intersect with the back side of the triangles after
entering the object. Therefore, RAY_FLAG_CULL_BACK_FACING_TRIANGLES is no longer
used.

### 7.2. Final Output

![Dielectrics](Images/7_Dielectrics1.png)

## 8. Depth of Field Blur

**Tutorial Class**: CameraTutorial

**Scene File**: CameraTutorialScene

For a detailed explanation of the algorithm, refer to the original text. Here,
we focus on the DXR implementation.

### 8.1. C# Code

The **FocusCamera** class adds focusDistance and aperture parameters to the
camera.

```csharp
thisCamera = GetComponent<Camera>();
var theta = thisCamera.fieldOfView * Mathf.Deg2Rad;
var halfHeight = math.tan(theta * 0.5f);
var halfWidth = thisCamera.aspect * halfHeight;
leftBottomCorner = transform.position + transform.forward * focusDistance -
                   transform.right * focusDistance * halfWidth -
                   transform.up * focusDistance * halfHeight;
size = new Vector2(focusDistance * halfWidth * 2.0f, focusDistance * halfHeight * 2.0f);
```

* **leftBottomCorner** is the world-space coordinate of the camera’s film plane’s
bottom-left corner.

* **size** is the size of the camera’s film plane in world space.

```csharp
cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraLeftBottomCorner, focusCamera.LeftBottomCorner);
cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraRight, focusCamera.transform.right);
cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraUp, focusCamera.transform.up);
cmd.SetRayTracingVectorParam(m_Shader, s_FocusCameraSize, focusCamera.Size);
cmd.SetRayTracingFloatParam(m_Shader, s_FocusCameraHalfAperture, focusCamera.Aperture * 0.5f);

cmd.SetRayTracingShaderPass(m_Shader, "RayTracing");
cmd.SetRayTracingAccelerationStructure(m_Shader, RayTracingRenderPipeline.s_AccelerationStructure, accelerationStructure);
cmd.SetRayTracingIntParam(m_Shader, s_FrameIndex, m_FrameIndex);
cmd.SetRayTracingBufferParam(m_Shader, s_PRNGStates, PRNGStates);
cmd.SetRayTracingTextureParam(m_Shader, s_OutputTarget, outputTarget);
cmd.SetRayTracingVectorParam(m_Shader, s_OutputTargetSize, outputTargetSize);
cmd.DispatchRays(m_Shader, "CameraRayGenShader", (uint) outputTarget.rt.width, (uint) outputTarget.rt.height, 1, camera);
```

### 8.2. RayTrace Shader

The RayTrace Shader is similar to the one from the previous sections, except
that _GenerateCameraRayWithOffset_ is replaced with GenerateFocusCameraRayWithOffset.

```glsl
inline void GenerateFocusCameraRayWithOffset(out float3 origin, out float3 direction, float2 apertureOffset, float2 offset)
{
  float2 xy = DispatchRaysIndex().xy + offset;
  float2 uv = xy / DispatchRaysDimensions().xy;

  float3 world = _FocusCameraLeftBottomCorner + uv.x * _FocusCameraSize.x * _FocusCameraRight + uv.y * _FocusCameraSize.y * _FocusCameraUp;
  origin = _WorldSpaceCameraPos.xyz + _FocusCameraHalfAperture * apertureOffset.x * _FocusCameraRight + _FocusCameraHalfAperture * apertureOffset.y * _FocusCameraUp;
  direction = normalize(world.xyz - origin);
}

float2 apertureOffset = GetRandomInUnitDisk(PRNGStates);
float2 offset = float2(GetRandomValue(PRNGStates), GetRandomValue(PRNGStates));
GenerateFocusCameraRayWithOffset(origin, direction, apertureOffset, offset);
```

The algorithm follows the same principles as in the original text, with some
calculations moved to the C# stage (as shown in section 8.1).

* The **apertureOffset** is generated by *GetRandomInUnitDisk*.

* The **GetRandomInUnitDisk** function generates a uniformly distributed random
  vector on a unit disk.

### 8.3. Final Output

![Focus Camera](Images/8_FocusCamera1.png)

## 9. Bringing It All Together

![Effect](Images/9_Final1.png)
![Effect](Images/9_Final2.png)