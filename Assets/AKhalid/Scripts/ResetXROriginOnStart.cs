using System;
using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class ResetXROriginOnStart : MonoBehaviour
{
    public Transform target;
    public XROrigin origin;
    public Camera xrCamera; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitForTrackedPoseDriver());
    }

    private IEnumerator WaitForTrackedPoseDriver()
    {

        Vector3 initialCameraPosition = xrCamera.transform.localPosition;
        yield return null; // Wait for one frame

        // Continue waiting if the camera's position is still being updated
        while (xrCamera.transform.localPosition == initialCameraPosition)
        {
            yield return null; // Wait for the next frame
        }

        yield return null;
        // Update the XR Origin's position
        Recenter();
    }
    
    public void Recenter()
    {
            origin.MoveCameraToWorldLocation(target.position);
            origin.MatchOriginUpCameraForward(target.up,target.forward);


    }
}
