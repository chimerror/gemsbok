using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorReflection : MonoBehaviour
{
    public bool DisablePixelLights = true;
    public int TextureSize = 256; // Must be power of two
    public float ClipPlaneOffset = 0.07f;

    public LayerMask ReflectLayers = -1;

    private Dictionary<int, Camera> _reflectionCameras = new Dictionary<int, Camera>(); // From other cameras' instance ids to reflection cameras

    private RenderTexture _reflectionTexture = null;
    private int _oldReflectionTextureSize = 0; // TODO: Can't we get this from _reflectionTexture?

    private static bool _InsideRendering = false;

    private void OnWillRenderObject()
    {
        var renderer = GetComponent<Renderer>();
        if (!enabled || !renderer || !renderer.sharedMaterial || !renderer.enabled)
        {
            return;
        }

        Camera camera = Camera.current;
        if (!camera)
        {
            return;
        }

        // TODO: This is static, so only one mirror can render. Plus, there may be a
        //       better way to avoid this than a boolean lock.
        if (_InsideRendering)
        {
            return;
        }
        _InsideRendering = true;


        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (DisablePixelLights)
        {
            QualitySettings.pixelLightCount = 0;
        }

        EnsureReflectionTexture();
        Camera reflectionCamera = UpdateReflectionCamera(camera);

        // Calculate the normal of the reflection plane encoding the position of the mirror into the homogeneous part.
        Vector3 position = transform.position;
        Vector3 normal = transform.up;
        float dotProduct = -Vector3.Dot(normal, position) - ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, dotProduct);

        // Now the reflection matrix can be calculated, and used to create the matrices of the reflection camera.
        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldPosition = camera.transform.position;
        Vector3 newPosition = reflection.MultiplyPoint(oldPosition);
        reflectionCamera.worldToCameraMatrix = camera.worldToCameraMatrix * reflection;
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, position, normal, 1.0f);
        Matrix4x4 projection = camera.CalculateObliqueMatrix(clipPlane);
        reflectionCamera.projectionMatrix = projection;

        // We use the "water" layer to represent any reflective surface, including other mirrors. We don't want to
        // get stuck rendering them infinitely.
        reflectionCamera.cullingMask = ~(1 << 4) & ReflectLayers.value;
        reflectionCamera.targetTexture = _reflectionTexture;

        // Must invert backface culling before rendering to the texture since what's back will be front and vice versa.
        GL.invertCulling = true;
        reflectionCamera.transform.position = newPosition;
        Vector3 eulerAngles = camera.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(0, eulerAngles.y, eulerAngles.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldPosition;
        GL.invertCulling = false;

        // TODO: Too much in this method, and not really sure how this can be broken up better. Lots of little
        //       variables that we only use once and then move on.

        // TODO: These materials are shared between all mirrors. I don't think we want that.
        Material[] materials = renderer.sharedMaterials;
        foreach (Material material in materials)
        {
            if (material.HasProperty("_ReflectionTex"))
            {
                material.SetTexture("_ReflectionTex", _reflectionTexture);
            }
        }

        if (DisablePixelLights)
        {
            QualitySettings.pixelLightCount = oldPixelLightCount;
        }

        _InsideRendering = false;
    }

    private void EnsureReflectionTexture()
    {
        if (!_reflectionTexture || _oldReflectionTextureSize != TextureSize)
        {
            if (_reflectionTexture)
            {
                DestroyImmediate(_reflectionTexture);
            }
            _reflectionTexture = new RenderTexture(TextureSize, TextureSize, 16);
            _reflectionTexture.name = "__MirrorReflection" + GetInstanceID();
            _reflectionTexture.isPowerOfTwo = true; // TODO: We assume it's a power of two
            _reflectionTexture.hideFlags = HideFlags.DontSave;
            _oldReflectionTextureSize = TextureSize;
        }
    }

    private Camera UpdateReflectionCamera(Camera currentCamera)
    {
        // Create a new reflection camera if we need to.
        Camera reflectionCamera = null;
        var cameraInstanceId = currentCamera.GetInstanceID();
        if (!_reflectionCameras.TryGetValue(cameraInstanceId, out reflectionCamera))
        {
            GameObject go = new GameObject(
                "Mirror Reflection Camera for mirror" + GetInstanceID() + " based on camera " + cameraInstanceId,
                typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCameras.Add(cameraInstanceId, reflectionCamera);
        }

        // either way, we'll need to update the reflection camera to match the currentCamera camera, since it may have changed.
        reflectionCamera.clearFlags = currentCamera.clearFlags;
        reflectionCamera.backgroundColor = currentCamera.backgroundColor;
        if (currentCamera.clearFlags == CameraClearFlags.Skybox)
        {
            var currentCameraSkybox = currentCamera.GetComponent<Skybox>();
            var reflectionCameraSkybox = reflectionCamera.GetComponent<Skybox>();
            if (!currentCamera || !currentCameraSkybox || !currentCameraSkybox.material) // Scene camera may not have actual skybox component!
            {
                reflectionCameraSkybox.enabled = false;
            }
            else
            {
                reflectionCameraSkybox.enabled = true;
                reflectionCameraSkybox.material = currentCameraSkybox.material;
            }
        }

        reflectionCamera.farClipPlane = currentCamera.farClipPlane;
        reflectionCamera.nearClipPlane = currentCamera.nearClipPlane;
        reflectionCamera.orthographic = currentCamera.orthographic;
        reflectionCamera.fieldOfView = currentCamera.fieldOfView;
        reflectionCamera.aspect = currentCamera.aspect;
        reflectionCamera.orthographicSize = currentCamera.orthographicSize;

        return reflectionCamera;
    }

    private Vector4 CameraSpacePlane(Camera camera, Vector3 position, Vector3 normal, float sideSign)
    {
        Vector3 offsetPosition = position + normal * ClipPlaneOffset;
        Matrix4x4 cameraMatrix = camera.worldToCameraMatrix;
        Vector3 cameraPosition = cameraMatrix.MultiplyPoint(offsetPosition);
        Vector3 cameraNormal = cameraMatrix.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
    }

    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
    {
        reflectionMatrix.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMatrix.m01 = (-2F * plane[0] * plane[1]);
        reflectionMatrix.m02 = (-2F * plane[0] * plane[2]);
        reflectionMatrix.m03 = (-2F * plane[3] * plane[0]);

        reflectionMatrix.m10 = (-2F * plane[1] * plane[0]);
        reflectionMatrix.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMatrix.m12 = (-2F * plane[1] * plane[2]);
        reflectionMatrix.m13 = (-2F * plane[3] * plane[1]);

        reflectionMatrix.m20 = (-2F * plane[2] * plane[0]);
        reflectionMatrix.m21 = (-2F * plane[2] * plane[1]);
        reflectionMatrix.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMatrix.m23 = (-2F * plane[3] * plane[2]);

        reflectionMatrix.m30 = 0F;
        reflectionMatrix.m31 = 0F;
        reflectionMatrix.m32 = 0F;
        reflectionMatrix.m33 = 1F;
    }

    private void OnDisable()
    {
        if (_reflectionTexture)
        {
            DestroyImmediate(_reflectionTexture);
            _reflectionTexture = null;
        }

        foreach (Camera camera in _reflectionCameras.Values)
        {
            DestroyImmediate(camera.gameObject);
        }
        _reflectionCameras.Clear();
    }

}
