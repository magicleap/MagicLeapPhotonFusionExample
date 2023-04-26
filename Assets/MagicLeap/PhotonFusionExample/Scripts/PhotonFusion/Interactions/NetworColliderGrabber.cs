using Fusion;
using MagicLeap.Networking;
using UnityEngine;

namespace MagicLeap.Networking.Interactions
{
    /// <summary>
    /// Modified version of script "NetworkHandColliderGrabber": from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Allows a Network Controller to grab <see cref="NetworkColliderGrabbable"/> objects.
    /// </para>
    /// <para>
    /// Position synchronization is handled in the <see cref="NetworkRig"/>.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(NetworkGameController))]
    [OrderAfter(typeof(NetworkGameController))]
    public class NetworColliderGrabber : NetworkBehaviour
    {
        [Networked]
        private NetworkColliderGrabbable _grabbedObject { get; set; }
        public NetworkGameController hand;
        private Collider _lastCheckedCollider;
        private NetworkColliderGrabbable _lastCheckColliderGrabbable;
        private void Awake()
        {
            hand = GetComponentInParent<NetworkGameController>();
        }



        private void OnTriggerStay(Collider other)
        {
         
            // We only trigger grabbing for our local hands
            if (!hand.IsLocalNetworkRig || !hand.LocalHardwareGameController) return;

            // Exit if an object is already grabbed
            if (_grabbedObject != null)
            {
                // It is already the grabbed object or another, but we don't allow shared grabbing here
                return;
            }

            NetworkColliderGrabbable grabbable;

            if (_lastCheckedCollider == other)
            {
                grabbable = _lastCheckColliderGrabbable;
            } 
            else
            {
                grabbable = other.GetComponentInParent<NetworkColliderGrabbable>();
            }
            // To limit the number of GetComponent calls, we cache the latest checked collider grabbable result
            _lastCheckedCollider = other;
            _lastCheckColliderGrabbable = grabbable;
            if (grabbable != null)
            {
           
                if (hand.LocalHardwareGameController.IsGrabbing) Grab(grabbable);
            } 
        }

        /// <summary>
        /// Ask the grabbable object to start following the hand
        /// </summary>
        /// <param name="grabbable"></param>
        public void Grab(NetworkColliderGrabbable grabbable)
        {
            Debug.Log($"Try to grab object {grabbable.gameObject.name} with {gameObject.name}");
            grabbable.Grab(this);
            _grabbedObject = grabbable;
        }

        /// <summary>
        /// Ask the grabbable object to stop following the hand
        /// </summary>
        /// <param name="grabbable"></param>
        public void Ungrab(NetworkColliderGrabbable grabbable)
        {
            Debug.Log($"Try to ungrab object {grabbable.gameObject.name} with {gameObject.name}");
            _grabbedObject.Ungrab();
            _grabbedObject = null;
        }

        
        private void Update()
        {
            if (!hand.IsLocalNetworkRig || !hand.LocalHardwareGameController) return;

            // Check if the local hand is still grabbing the object
            if (_grabbedObject != null && !hand.LocalHardwareGameController.IsGrabbing)
            {
                // Object released by this hand
                Ungrab(_grabbedObject);
            }
        }
    }
}
