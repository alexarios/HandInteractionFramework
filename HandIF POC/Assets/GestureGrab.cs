using UnityEngine;
using System.Collections;

public class GestureGrab : OVRGrabber
{
    // Boolean used to check if you are grabbing.
    [SerializeField]
    bool isGrabbing = false;

    // Boolean used to check if hand is closed.
    [SerializeField]
    bool handClosed = false;

    [SerializeField]
    OVRCustomSkeleton skeleton;

    protected override void Start()
    {
        // Use base.Start to instantiate things like m_lastPos, m_last_Rot and the m_parentTransform.
        base.Start();

        if (skeleton._enablePhysicsCapsules)
        {
            StartCoroutine(InitRoutine());
        }
        
    }

    // Waits until hands are initialized before setting capsule volumes.
    public IEnumerator InitRoutine()
    {
        while (!skeleton.IsInitialized)
        {
            yield return null;
        }
        SetCapsuleVolumes();
    }

    // Adds all grab capsules to m_grabVolumes.
    private void SetCapsuleVolumes()
    {
        m_grabVolumes = new Collider[skeleton._skeleton.NumBoneCapsules];
        for (int i = 0; i < m_grabVolumes.Length; ++i)
        {
            m_grabVolumes[i] = skeleton.GrabCapsules[i];
        }
    }

    // Sets isGrabbing variables to true/false.
    public void DetectGrabbing(string _isGrabbing)
    {
        if (_isGrabbing.Equals("true"))
        {
            isGrabbing = true;
        }
        // Also sets handClosed to false when not grabbing.
        else if (_isGrabbing.Equals("false"))
        {
            isGrabbing = false;
            handClosed = false;
        }
    }

    public override void Update()
    {
        base.Update();

        // Checks to see if hand is grabbing and isn't currently holding an object.
        if (!m_grabbedObj && isGrabbing)
        {
            // If there is a valid grabCandidate and the hand is open before grabbing, begins the grab.
            if (m_grabCandidates.Count > 0 && handClosed == false)
            {
                GrabBegin();
            }
            else
            {
                handClosed = true;
            }
        }
        // If there is an object being held and hand is no longer grabbing, we call GrabEnd().
        else if (m_grabbedObj != null && !isGrabbing)
        {
            GrabEnd();
        }
    }

    protected override void GrabEnd()
    {

        // If we are grabbing an object.
        if (m_grabbedObj != null)
        {
            // Calculates the linear velocity of the hand.
            Vector3 linearVelocity = (m_parentTransform.position - m_lastPos) / Time.fixedDeltaTime;
            // Calculates the angular velocity of the hand.
            Vector3 angularVelocity = (m_parentTransform.eulerAngles - m_lastRot.eulerAngles) / Time.fixedDeltaTime;
            
            // Call OvrGrabber's GrabbableRelease w/ our linear and angular velocities.      
            GrabbableRelease(linearVelocity, angularVelocity);
        }
        // Restores the colliders used for grabbing
        GrabVolumeEnable(true);
    }
}
