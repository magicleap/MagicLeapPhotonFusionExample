
using UnityEngine;

namespace MagicLeapNetworkingDemo.Grabbing {

    /// <summary>
    /// Modified version of script: from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    ///  Detect the presence of a <see cref="Grabbable"/> under the controller collider.
    /// </para>
    /// <para>
    ///  Trigger the grabbing/ungrabbing depending on the <see cref="HardwareGameController.IsGrabbing"/> field.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(HardwareGameController))]
    public class Grabber : MonoBehaviour
    {
        HardwareGameController gameController;

        Collider lastCheckedCollider;
        Grabbable lastCheckColliderGrabbable;
        public Grabbable grabbedObject;
        // Will be set by the NetworkGrabber for the local user itself, when it spawns
        public NetworkGrabber networkGrabber;

        private void Awake()
        {
            gameController = GetComponentInParent<HardwareGameController>();
        }

        private void OnTriggerStay(Collider other)
        {
            // Exit if an object is already grabbed
            if (grabbedObject != null)
            {
                // It is already the grabbed object or another, but we don't allow shared grabbing here
                return;
            }

            Grabbable grabbable;

            if (lastCheckedCollider == other)
            {
                grabbable = lastCheckColliderGrabbable;
            }
            else
            {
                grabbable = other.GetComponentInParent<Grabbable>();
            }
            // To limit the number of GetComponent calls, we cache the latest checked collider grabbable result
            lastCheckedCollider = other;
            lastCheckColliderGrabbable = grabbable;
            if (grabbable != null)
            {
                if (gameController.IsGrabbing) Grab(grabbable);
            }
        }

        public void Grab(Grabbable grabbable)
        {
            Debug.Log("Grabbed: "+ grabbable.name);
            grabbable.Grab(this);
            grabbedObject = grabbable;
        }

        public void Ungrab(Grabbable grabbable)
        {
            grabbedObject.Ungrab();
            grabbedObject = null;
        }
        private void Update()
        {
            // Check if the local hand is still grabbing the object
            if (grabbedObject != null && !gameController.IsGrabbing)
            {
                // Object released by this hand
                Ungrab(grabbedObject);
            }
        }
    }

}
