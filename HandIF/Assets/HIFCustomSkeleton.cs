using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-80)]
public class HIFCustomSkeleton : OVRSkeleton
{

    // Start is called before the first frame update
    public override void Start()
    {
        HasGrabCapsules = true;
        if (ShouldInitialize())
        {
            Debug.Log("HIF Start");
            Initialize();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        for (int i = 0; i < _capsules.Count; ++i)
        {
            OVRBoneCapsule capsule = _capsules[i];
            CapsuleCollider grabCollider = GrabCapsules[i].CapsuleCollider;

            grabCollider.transform.position = capsule.CapsuleCollider.transform.position;
            grabCollider.transform.rotation = capsule.CapsuleCollider.transform.rotation;
        }
    }
}
