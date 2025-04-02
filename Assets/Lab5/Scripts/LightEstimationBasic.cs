using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;

public class LightEstimationBasic : MonoBehaviour
{
    [SerializeField] private ARCameraManager _cameraManager;
    private Light _mainLight;

    public float? Brightness { get; private set; }
    public float? ColorTemperature { get; private set; }
    public Color? ColorCorrection { get; private set; }
    public Vector3? MainLightDirection { get; private set; }
    public Color? MainLightColor { get; private set; }
    public float? MainLightIntensityLumens { get; private set; }
    public SphericalHarmonicsL2? SphericalHarmonics { get; private set; }

    private void Awake()
    {
        _mainLight = GetComponent<Light>();
        Debug.Log("[Light] Awake: Main light component retrieved.");
    }

    private void OnEnable()
    {
        if (_cameraManager != null)
        {
            _cameraManager.frameReceived += FrameChanged;
            Debug.Log("[Light] OnEnable: Subscribed to frameReceived event.");
        }
        else
        {
            Debug.LogError("[ERROR] OnEnable: ARCameraManager is not assigned.");
        }
    }

    private void OnDisable()
    {
        if (_cameraManager != null)
        {
            _cameraManager.frameReceived -= FrameChanged;
            Debug.Log("[Light] OnDisable: Unsubscribed from frameReceived event.");
        }
    }

    private void FrameChanged(ARCameraFrameEventArgs args)
    {
        Debug.Log("[Light] FrameChanged: New frame received. " + args.ToString());

        if (args.lightEstimation.averageBrightness.HasValue)
        {
            Brightness = args.lightEstimation.averageBrightness.Value;
            _mainLight.intensity = Brightness.Value;
            Debug.Log($"[Light] FrameChanged: Brightness updated to {Brightness.Value}");
        }

        if (args.lightEstimation.averageColorTemperature.HasValue)
        {
            ColorTemperature = args.lightEstimation.averageColorTemperature.Value;
            _mainLight.colorTemperature = ColorTemperature.Value;
            Debug.Log($"[Light] FrameChanged: ColorTemperature updated to {ColorTemperature.Value}");
        }

        if (args.lightEstimation.colorCorrection.HasValue)
        {
            ColorCorrection = args.lightEstimation.colorCorrection.Value;
            _mainLight.color = ColorCorrection.Value;
            Debug.Log($"[Light] FrameChanged: ColorCorrection updated to {ColorCorrection.Value}");
        }

        if (args.lightEstimation.mainLightDirection.HasValue)
        {
            MainLightDirection = args.lightEstimation.mainLightDirection;
            _mainLight.transform.rotation = Quaternion.LookRotation(MainLightDirection.Value);
            Debug.Log($"[Light] FrameChanged: MainLightDirection updated to {MainLightDirection.Value}");
        }

        if (args.lightEstimation.mainLightColor.HasValue)
        {
            MainLightColor = args.lightEstimation.mainLightColor;

            // ARCore needs to apply energy conservation term (1 / PI) and be placed in gamma
            _mainLight.color = MainLightColor.Value / Mathf.PI;
            _mainLight.color = _mainLight.color.gamma;
            Debug.Log($"[Light] FrameChanged: MainLightColor updated to {MainLightColor.Value}");
        }

        if (args.lightEstimation.mainLightIntensityLumens.HasValue)
        {
            MainLightIntensityLumens = args.lightEstimation.mainLightIntensityLumens;
            _mainLight.intensity = args.lightEstimation.averageMainLightBrightness.Value;
            Debug.Log($"[Light] FrameChanged: MainLightIntensityLumens updated to {MainLightIntensityLumens.Value}");
        }

        if (args.lightEstimation.ambientSphericalHarmonics.HasValue)
        {
            SphericalHarmonics = args.lightEstimation.ambientSphericalHarmonics;
            RenderSettings.ambientMode = AmbientMode.Skybox;
            RenderSettings.ambientProbe = SphericalHarmonics.Value;
            Debug.Log("[Light] FrameChanged: SphericalHarmonics updated.");
        }
    }
}