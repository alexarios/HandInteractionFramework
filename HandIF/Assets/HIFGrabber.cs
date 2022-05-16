using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(201)]
public class HIFGrabber : OVRGrabber
{
    [SerializeField]
    public HIFCustomSkeleton HIFSkeleton;

    // List of strings for capsules that are considered active for grabbing.
    protected static List<string> ActiveCapsuleNames = new List<string>(new string[] { "3", "WristRoot" });

    // List of all CapsuleColliders currently touching an object.
    public HashSet<GameObject> ActiveGrabCapsules = new HashSet<GameObject>();


    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        StartCoroutine(InitRoutine());
    }

    // Waits for HIFSkeleton to initialize.
    public IEnumerator InitRoutine()
    {
        while (!HIFSkeleton.IsInitialized)
        {
            yield return null;
        }
        AddGrabVolumes();
        AddGrabCapsuleTriggers();
    }
    override public void Update()
    {
        if (m_operatingWithoutOVRCameraRig)
        {
            OnUpdatedAnchors();
        }
    }
    // Populate m_grabVolumes w/ all GrabCapsules in the hand.
    protected void AddGrabVolumes()
    {
        if (HIFSkeleton.GrabCapsules.Count > 0)
        {
            int i = 0;
            m_grabVolumes = new CapsuleCollider[HIFSkeleton.GrabCapsules.Count - 3];
            foreach (GrabCapsule capsule in HIFSkeleton.GrabCapsules)
            {
                if (capsule.BoneIndex < 2 || capsule.BoneIndex > 5)
                {
                    m_grabVolumes[i] = capsule.CapsuleCollider;
                    i++;
                }  
            }
        }
    }

    // Adds GrabCapsuleTrigger components to all GrabCapsules that are in ActiveCapsuleNames.
    protected void AddGrabCapsuleTriggers()
    {
        foreach (GrabCapsule currCapsule in HIFSkeleton.GrabCapsules)
        {
            if (IsActiveCapsule(currCapsule.CapsuleCollider))
            {
                GrabCapsuleTrigger currTrigger = currCapsule.CapsuleCollider.gameObject.AddComponent<GrabCapsuleTrigger>();
                currTrigger.SetParentGrabber();
            }
        }
    }

    // Returns if the given capsule is in ActiveCapsuleNames.
    public bool IsActiveCapsule(CapsuleCollider capsule)
    {
        foreach (string currName in ActiveCapsuleNames)
        {
            if (capsule.name.Contains(currName))
            {
                return true;
            }
        }
        return false;
    }

    // Checks CandidateGrabVolumes and returns if a grab should be initiated.
    public bool CanGrab()
    {
        if (ActiveGrabCapsules.Count > 1)
        {
            foreach (GameObject currCapsule in ActiveGrabCapsules)
            {
                if (currCapsule.name.Contains("Thumb"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public override void OnUpdatedAnchors()
    {
        if (HIFSkeleton._skeletonType == OVRSkeleton.SkeletonType.HandLeft)
        {
            //Debug.Log(HIFSkeleton._skeletonType + " " + ActiveGrabCapsules.Count);
        }
        
        Vector3 destPos = m_parentTransform.TransformPoint(m_anchorOffsetPosition);
        Quaternion destRot = m_parentTransform.rotation * m_anchorOffsetRotation;

        if (m_moveHandPosition)
        {
            GetComponent<Rigidbody>().MovePosition(destPos);
            GetComponent<Rigidbody>().MoveRotation(destRot);
        }

        if (!m_parentHeldObject)
        {
            MoveGrabbedObject(destPos, destRot);
        }

        m_lastPos = transform.position;
        m_lastRot = transform.rotation;
    }

    // Overloaded CheckForGrabOrRelease
    public bool CheckForGrabOrRelease()
    {
        if (CanGrab())
        {
            RigidbodySwitch(false);
            GrabBegin();
            return true;
        }
        else
        {
            GrabEnd();
            RigidbodySwitch(true);
            return false;
        }
    }

    // Disables/reenables hand rigidbody to not interfere with grabbing.
    protected void RigidbodySwitch(bool enableRB)
    {
        foreach (OVRBoneCapsule currBoneCapsule in HIFSkeleton._capsules)
        {
            if (enableRB)
            {
                currBoneCapsule.CapsuleRigidbody.detectCollisions = true;
            } else
            {
                currBoneCapsule.CapsuleRigidbody.detectCollisions = false;
            }
        }
    }

    // TODO: Calculate the correct velocity of the object after letting go
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
            GrabbableRelease(Vector3.zero, Vector3.zero);
        }
        // Restores the colliders used for grabbing
        GrabVolumeEnable(true);
    }
}
