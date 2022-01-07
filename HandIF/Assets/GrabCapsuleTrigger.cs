using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Makes sure rest of OVR scripts execute first.
[DefaultExecutionOrder(202)]
public class GrabCapsuleTrigger : MonoBehaviour
{

    // HIFGrabber component attached to this object.
    [SerializeField]
    HIFGrabber hifGrabber;

    public bool CanGrab = true;

    public void SetParentGrabber()
    {
        hifGrabber = gameObject.transform.parent.parent.GetComponent<HIFGrabber>();
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (hifGrabber != null && other.GetComponent<OVRGrabbable>() != null && CanGrab)
        {
            hifGrabber.ActiveGrabCapsules.Add(gameObject);
            if (hifGrabber.CheckForGrabOrRelease())
            {
                CanGrab = false;
            }
        } 
    }

    protected void OnTriggerExit(Collider other)
    {
        if (hifGrabber != null && other.GetComponent<OVRGrabbable>() != null)
        {
            hifGrabber.ActiveGrabCapsules.Remove(gameObject);
            if (!hifGrabber.CheckForGrabOrRelease())
            {
                CanGrab = true;
            }
        }
    }
}
