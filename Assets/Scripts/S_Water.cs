using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class DisturbingObject
{
    public GameObject myObject;
    public Vector3 pos;
    public float radius;
    public float destRadius;
    public float amplitude;
    public float destAmplitude;
    public Vector3 speed;
    public Vector3 destSpeed;
    public float IdleTimer = 0f;
}

[ExecuteInEditMode]
public class S_Water : MonoBehaviour
{

    public LayerMask UsingLayers;

    public bool m_DisablePixelLights = true;
    public int m_TextureSize = 256;
    public float m_ClipPlaneOffset = 0.07f;

    private Hashtable m_ReflectionCameras = new Hashtable(); // Camera -> Camera table

    private RenderTexture m_ReflectionTexture = null;
    private int m_OldReflectionTextureSize = 0;

    private static bool s_InsideRendering = false;

    [Space(10)]
    public float walkRadius = 4f;
    public float idleRadius = 2f;
    [Space(10)]
    public float walkAmplitude = 0.6f;
    public float idleAmplitude = 0.3f;
    [Space(10)]
    public float spreadSpeed = 1f;
    public float amplitudeChangeSpeed = 1f;
    public float speedOfChangingSpeed = 2f;
    public float speedOfChangingWaveSpeed = 2f;
    [Space(10)]
    public float IdleTime = 2f;


    private List<DisturbingObject> DistObjects = new List<DisturbingObject>();


    // Update is called once per frame
    void Update()
    {
        bool ChangingStuff = false;

        if (DistObjects.Count > 0)
            for (int i = 0; i < DistObjects.Count; ++i)
            {
                if (DistObjects[i].myObject == null)
                    if (DistObjects[i].amplitude < 0.01f)
                    {
                        ChangingStuff = true;

                        DistObjects.RemoveAt(i);
                        --i;
                        continue;
                    }
                    else
                    {
                        if (DistObjects[i].destAmplitude != 0f)
                        {
                            ChangingStuff = true;
                            DistObjects[i].destAmplitude = 0f;
                        }
                    }
                else
                {
                    // Moving
                    if (DistObjects[i].pos != DistObjects[i].myObject.transform.position)
                    {
                        if (DistObjects[i].destRadius != walkRadius)
                        {
                            DistObjects[i].destRadius = walkRadius;

                        }
                        if (DistObjects[i].destAmplitude != walkAmplitude)
                        {
                            DistObjects[i].destAmplitude = walkAmplitude;

                        }

                        DistObjects[i].IdleTimer = IdleTime;
                        DistObjects[i].destSpeed = (DistObjects[i].pos - DistObjects[i].myObject.transform.position) / Time.deltaTime;
                        DistObjects[i].destSpeed.y = 0f;
                        DistObjects[i].pos = DistObjects[i].myObject.transform.position;
                        ChangingStuff = true;
                    }
                    else // Standing still
                    {
                        if (DistObjects[i].IdleTimer == 0)
                        {
                            if (DistObjects[i].destRadius != idleRadius)
                            {
                                DistObjects[i].destRadius = idleRadius;
                                ChangingStuff = true;
                            }
                            if (DistObjects[i].destAmplitude != idleAmplitude)
                            {
                                DistObjects[i].destAmplitude = idleAmplitude;
                                ChangingStuff = true;
                            }

                        }

                        if (DistObjects[i].destSpeed != Vector3.zero)
                        {
                            DistObjects[i].destSpeed = Vector3.zero;
                            ChangingStuff = true;
                        }
                    }

                }

                if (DistObjects[i].IdleTimer > 0f)
                {
                    DistObjects[i].IdleTimer -= Time.deltaTime;
                    if (DistObjects[i].IdleTimer < 0)
                    {
                        DistObjects[i].IdleTimer = 0;
                        DistObjects[i].destRadius = idleRadius;
                        DistObjects[i].destAmplitude = idleAmplitude;
                    }
                }

                // radius change
                if (DistObjects[i].radius < DistObjects[i].destRadius)
                {
                    DistObjects[i].radius += Time.deltaTime * spreadSpeed;
                    if (DistObjects[i].radius > DistObjects[i].destRadius)
                        DistObjects[i].radius = DistObjects[i].destRadius;

                    ChangingStuff = true;
                }

                if (DistObjects[i].radius > DistObjects[i].destRadius)
                {
                    DistObjects[i].radius -= Time.deltaTime * spreadSpeed / 3;
                    if (DistObjects[i].radius < DistObjects[i].destRadius)
                        DistObjects[i].radius = DistObjects[i].destRadius;

                    ChangingStuff = true;
                }

                // amplitude change
                if (DistObjects[i].amplitude < DistObjects[i].destAmplitude)
                {
                    DistObjects[i].amplitude += Time.deltaTime * spreadSpeed;
                    if (DistObjects[i].amplitude > DistObjects[i].destAmplitude)
                        DistObjects[i].amplitude = DistObjects[i].destAmplitude;

                    ChangingStuff = true;
                }

                if (DistObjects[i].amplitude > DistObjects[i].destAmplitude)
                {
                    DistObjects[i].amplitude -= Time.deltaTime * amplitudeChangeSpeed / 3;
                    if (DistObjects[i].amplitude < DistObjects[i].destAmplitude)
                        DistObjects[i].amplitude = DistObjects[i].destAmplitude;

                    ChangingStuff = true;
                }

                // speed vector change
                if (DistObjects[i].speed != DistObjects[i].destSpeed)
                {
                    DistObjects[i].speed = Vector3.MoveTowards(DistObjects[i].speed, DistObjects[i].destSpeed, Time.deltaTime * speedOfChangingSpeed);
                    ChangingStuff = true;
                }

            }


        if (ChangingStuff)
        {
            Color[] PosArray = new Color[10];
            Color[] SpeedArray = new Color[10];
            int Length = DistObjects.Count < 10 ? DistObjects.Count : 10;

            for (int i = 0; i < Length; ++i)
            {
                PosArray[i].r = (DistObjects[i].pos.x - transform.position.x) / transform.localScale.x;
                PosArray[i].g = (DistObjects[i].pos.z - transform.position.z) / transform.localScale.z;
                PosArray[i].b = DistObjects[i].radius / transform.localScale.x;
                PosArray[i].a = DistObjects[i].amplitude;
                SpeedArray[i].r = DistObjects[i].speed.x;
                SpeedArray[i].g = DistObjects[i].speed.z;
                SpeedArray[i].b = DistObjects[i].speed.magnitude;
            }

            Material MyMat = GetComponent<Renderer>().material;
            MyMat.SetColorArray("_DistArray", PosArray);
            MyMat.SetColorArray("_SpeedArray", SpeedArray);
            MyMat.SetFloat("_ArrayLength", Length);
           
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (UsingLayers == (UsingLayers | (1 << other.gameObject.layer)))
        {

            DisturbingObject NewOne = new DisturbingObject();

            NewOne.myObject = other.gameObject;
            NewOne.pos = other.transform.position;


            NewOne.destRadius = walkRadius;
            NewOne.radius = 0f;
            NewOne.destAmplitude = walkAmplitude;
            NewOne.amplitude = 0f;
            NewOne.IdleTimer = IdleTime;

            DistObjects.Add(NewOne);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        int l = DistObjects.Count;
        for (int i = 0; i < l; ++i)
            if (DistObjects[i].myObject == other.gameObject)
            {
                DistObjects[i].myObject = null;
                return;
            }
    }

    // This is called when it's known that the object will be rendered by some
    // camera. We render reflections and do other updates here.
    // Because the script executes in edit mode, reflections for the scene view
    // camera will just work!
    public void OnWillRenderObject()
    {
        var rend = GetComponent<Renderer>();
        if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
            return;

        Camera cam = Camera.current;
        if (!cam)
            return;

        // Safeguard from recursive reflections.        
        if (s_InsideRendering)
            return;
        s_InsideRendering = true;

        Camera reflectionCamera;
        CreateMirrorObjects(cam, out reflectionCamera);

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        // Optionally disable pixel lights for reflection
        int oldPixelLightCount = QualitySettings.pixelLightCount;
        if (m_DisablePixelLights)
            QualitySettings.pixelLightCount = 0;

        UpdateCameraModes(cam, reflectionCamera);

        // Render reflection
        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - m_ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.zero;
        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = cam.transform.position;
        Vector3 newpos = reflection.MultiplyPoint(oldpos);
        reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
        //Matrix4x4 projection = cam.projectionMatrix;
        Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
        reflectionCamera.projectionMatrix = projection;

        reflectionCamera.cullingMask = ~(1 << 4) & UsingLayers.value; // never render water layer
        //reflectionCamera.cullingMask = m_ReflectLayers.value;
        reflectionCamera.targetTexture = m_ReflectionTexture;
        //GL.SetRevertBackfacing(true);
        GL.invertCulling = true;
        reflectionCamera.transform.position = newpos;
        Vector3 euler = cam.transform.eulerAngles;
        reflectionCamera.transform.eulerAngles = new Vector3(0, euler.y, euler.z);
        reflectionCamera.Render();
        reflectionCamera.transform.position = oldpos;
        //GL.SetRevertBackfacing(false);
        GL.invertCulling = false;
        Material[] materials = rend.sharedMaterials;
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_ReflectionTex"))
                mat.SetTexture("_ReflectionTex", m_ReflectionTexture);
        }

        // Restore pixel light count
        if (m_DisablePixelLights)
            QualitySettings.pixelLightCount = oldPixelLightCount;

        s_InsideRendering = false;
    }


    // Cleanup all the objects we possibly have created
    void OnDisable()
    {
        if (m_ReflectionTexture)
        {
            DestroyImmediate(m_ReflectionTexture);
            m_ReflectionTexture = null;
        }
        foreach (DictionaryEntry kvp in m_ReflectionCameras)
            DestroyImmediate(((Camera)kvp.Value).gameObject);
        m_ReflectionCameras.Clear();
    }


    private void UpdateCameraModes(Camera src, Camera dest)
    {
        if (dest == null)
            return;
        // set camera to clear the same way as current camera
        dest.clearFlags = src.clearFlags;
        dest.backgroundColor = src.backgroundColor;
        if (src.clearFlags == CameraClearFlags.Skybox)
        {
            Skybox sky = src.GetComponent(typeof(Skybox)) as Skybox;
            Skybox mysky = dest.GetComponent(typeof(Skybox)) as Skybox;
            if (!sky || !sky.material)
            {
                mysky.enabled = false;
            }
            else
            {
                mysky.enabled = true;
                mysky.material = sky.material;
            }
        }
        // update other values to match current camera.
        // even if we are supplying custom camera&projection matrices,
        // some of values are used elsewhere (e.g. skybox uses far plane)
        dest.farClipPlane = src.farClipPlane;
        dest.nearClipPlane = src.nearClipPlane;
        dest.orthographic = src.orthographic;
        dest.fieldOfView = src.fieldOfView;
        dest.aspect = src.aspect;
        dest.orthographicSize = src.orthographicSize;
    }

    // On-demand create any objects we need
    private void CreateMirrorObjects(Camera currentCamera, out Camera reflectionCamera)
    {
        reflectionCamera = null;

        // Reflection render texture
        if (!m_ReflectionTexture || m_OldReflectionTextureSize != m_TextureSize)
        {
            if (m_ReflectionTexture)
                DestroyImmediate(m_ReflectionTexture);
            m_ReflectionTexture = new RenderTexture(m_TextureSize, m_TextureSize, 16);
            m_ReflectionTexture.name = "__MirrorReflection" + GetInstanceID();
            m_ReflectionTexture.isPowerOfTwo = true;
            m_ReflectionTexture.hideFlags = HideFlags.DontSave;
            m_OldReflectionTextureSize = m_TextureSize;
        }

        // Camera for reflection
        reflectionCamera = m_ReflectionCameras[currentCamera] as Camera;
        if (!reflectionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            GameObject go = new GameObject("Mirror Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
            reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            reflectionCamera.transform.position = transform.position;
            reflectionCamera.transform.rotation = transform.rotation;
            reflectionCamera.gameObject.AddComponent<FlareLayer>();
            go.hideFlags = HideFlags.HideAndDontSave;
            m_ReflectionCameras[currentCamera] = reflectionCamera;
        }
    }

    // Extended sign: returns -1, 0 or 1 based on sign of a
    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    // Calculates reflection matrix around the given plane
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }


}
