using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Arena : MonoBehaviour
{
    [SerializeField]
    private Camera Cam;

    public static float Width { get; private set; }
    public static float Height { get; private set; }

    void Update()
    {
#if UNITY_EDITOR 
        if (!Application.isPlaying)
        {
            Calculate();
        }
#endif
    }

    public void Calculate()
    {
        if (Cam != null)
        {
            Height = CameraUtils.FrustumHeightAtDistance(Cam.farClipPlane - 1.0f, Cam.fieldOfView);
            Width = Height * Cam.aspect;
            transform.localScale = new Vector3(Width * 0.1f, transform.localScale.y, Height * 0.1f);
        }
    }
}
