using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class OculusXRSetup : MonoBehaviour
{
    public XRRayInteractor leftRay;
    public XRRayInteractor rightRay;

    void Start()
    {
        // Ensure ray interactors are enabled for UI
        leftRay.enableUIInteraction = true;
        rightRay.enableUIInteraction = true;
    }
}