using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR;
using Valve.VR;

public class VrMirrorReflection : MonoBehaviour
{
    public float ClipPlaneOffset = 0.07f;
    public int TextureSize = 1024;
    public Camera PlayerCamera;
    public Camera ReflectionCamera;
    private RenderTexture _leftEyeTexture;
    private RenderTexture _rightEyeTexture;

    private void Awake()
    {
        _leftEyeTexture = new RenderTexture(TextureSize, TextureSize, 24);
        _rightEyeTexture = new RenderTexture(TextureSize, TextureSize, 24);
    }

    private void OnWillRenderObject()
    {
        if (Camera.current != PlayerCamera)
        {
            return;
        }

        Material material = GetComponent<MeshRenderer>().sharedMaterial;

        Vector3 position = transform.position;
        Vector3 normal = transform.up;
        float dotProduct = -Vector3.Dot(normal, position) - ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, dotProduct);
        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 playerPosition = PlayerCamera.transform.position;
        Vector3 reflectedPlayerPosition = reflection.MultiplyPoint(playerPosition);
        ReflectionCamera.worldToCameraMatrix = PlayerCamera.worldToCameraMatrix * reflection;

        ReflectionCamera.transform.position = reflectedPlayerPosition;
        var playerRotation = PlayerCamera.transform.eulerAngles;
        ReflectionCamera.transform.eulerAngles = new Vector3(0, playerRotation.y, playerRotation.z);

        GL.invertCulling = true;

        // left eye
        var eyeOffset = SteamVR.instance.eyes[0].pos;
        eyeOffset.z = 0.0f;
        ReflectionCamera.transform.position = reflectedPlayerPosition + reflection.MultiplyPoint(eyeOffset);

        ReflectionCamera.projectionMatrix = SteamMatrixToUnityMatrix(
            SteamVR.instance.hmd.GetProjectionMatrix(EVREye.Eye_Left, PlayerCamera.nearClipPlane, PlayerCamera.farClipPlane, EGraphicsAPIConvention.API_DirectX));

        ReflectionCamera.targetTexture = _leftEyeTexture;
        ReflectionCamera.Render();
        material.SetTexture("_LeftEyeTexture", _leftEyeTexture);

        // right eye
        eyeOffset = SteamVR.instance.eyes[1].pos;
        eyeOffset.z = 0.0f;
        ReflectionCamera.transform.position = reflectedPlayerPosition + reflection.MultiplyPoint(eyeOffset);
        ReflectionCamera.projectionMatrix = SteamMatrixToUnityMatrix(
            SteamVR.instance.hmd.GetProjectionMatrix(EVREye.Eye_Right, PlayerCamera.nearClipPlane, PlayerCamera.farClipPlane, EGraphicsAPIConvention.API_DirectX));

        ReflectionCamera.targetTexture = _rightEyeTexture;
        ReflectionCamera.Render();
        material.SetTexture("_RightEyeTexture", _rightEyeTexture);

        GL.invertCulling = false;
    }

    private Matrix4x4 SteamMatrixToUnityMatrix(HmdMatrix44_t input)
    {
        var m = Matrix4x4.identity;

        m[0, 0] = input.m0;
        m[0, 1] = input.m1;
        m[0, 2] = input.m2;
        m[0, 3] = input.m3;

        m[1, 0] = input.m4;
        m[1, 1] = input.m5;
        m[1, 2] = input.m6;
        m[1, 3] = input.m7;

        m[2, 0] = input.m8;
        m[2, 1] = input.m9;
        m[2, 2] = input.m10;
        m[2, 3] = input.m11;

        m[3, 0] = input.m12;
        m[3, 1] = input.m13;
        m[3, 2] = input.m14;
        m[3, 3] = input.m15;

        return m;
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
}
