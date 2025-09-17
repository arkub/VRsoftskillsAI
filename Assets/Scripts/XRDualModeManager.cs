using UnityEngine;
using UnityEngine.XR.Management;
using System.Collections;

public class XRDualModeManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return null; // wait 1 frame so XRGeneralSettings is ready

        bool headsetConnected = UnityEngine.XR.XRSettings.isDeviceActive &&
                                UnityEngine.XR.XRSettings.loadedDeviceName.Contains("Oculus");

        var xrManager = XRGeneralSettings.Instance.Manager;

        // Stop and deinit any currently running loader
        xrManager.StopSubsystems();
        xrManager.DeinitializeLoader();

        if (headsetConnected)
        {
            Debug.Log("Oculus headset detected. Starting Oculus XR Loader...");
            xrManager.InitializeLoaderSync();
            if (xrManager.activeLoader != null && xrManager.activeLoader.name.Contains("Oculus"))
                xrManager.StartSubsystems();
        }
        else
        {
            Debug.Log("No headset detected. Starting Mock HMD for simulation...");
            xrManager.InitializeLoaderSync();
            if (xrManager.activeLoader != null && xrManager.activeLoader.name.Contains("MockHMD"))
                xrManager.StartSubsystems();
        }
    }
}