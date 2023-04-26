using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using MagicLeapNetworkingDemo.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MagicLeap.Networking.Interactions
{
    /// <summary>
    /// Modified version of script "NetworkHandColliderGrabbable" : from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The script was changed to not use rigidbodies.
    /// </para>
    /// </remarks>
    [OrderAfter(typeof(NetworColliderGrabber))]
    public class NetworkColliderGrabbable : NetworkBehaviour
    {
        public bool IsGrabbed => _currentGrabber != null;

        [Header("Events")]
        public UnityEvent OnDidUngrab = new UnityEvent();
        [Tooltip("Called only for the local grabber, when they may wait for authority before grabbing. onDidGrab will be called on all users")]
        public UnityEvent<NetworColliderGrabber> onWillGrab = new UnityEvent<NetworColliderGrabber>();
        public UnityEvent<NetworColliderGrabber> onDidGrab = new UnityEvent<NetworColliderGrabber>();

        [Networked(OnChanged = nameof(OnGrabberChanged))]
        private NetworColliderGrabber _currentGrabber { get; set; }
        [Networked] private Vector3 _localPositionOffset { get; set; }
        [Networked] private Quaternion _localRotationOffset { get; set; }

        private bool _extrapolateWhileTakingAuthority = true;
        private bool isTakingAuthority = false;
        private Vector3 _localPositionOffsetWhileTakingAuthority;
        private Quaternion _localRotationOffsetWhileTakingAuthority;
        private NetworColliderGrabber _grabberWhileTakingAuthority;
        private ColocationObject _networkTransform;

        public static void OnGrabberChanged(Changed<NetworkColliderGrabbable> changed)
        {
            // We load the previous state to find what was the grabber before
            changed.LoadOld();
            NetworColliderGrabber previousGrabber = null;
            if (changed.Behaviour._currentGrabber != null)
            {
                previousGrabber = changed.Behaviour._currentGrabber;
            }
            // We reload the current state to see the current grabber
            changed.LoadNew();

            if (previousGrabber)
            {
                changed.Behaviour.DidUngrab();
            }
            if (changed.Behaviour._currentGrabber)
            {
                changed.Behaviour.DidGrab();
            }
        }

        private void Awake()
        {
            _networkTransform = GetComponent<ColocationObject>();
        }

        public void Ungrab()
        {
            _currentGrabber = null;
        }

        public async void Grab(NetworColliderGrabber newGrabber)
        {
            if (onWillGrab != null) onWillGrab.Invoke(newGrabber);

            // Find grabbable position/rotation in grabber referential
            _localPositionOffsetWhileTakingAuthority = newGrabber.transform.InverseTransformPoint(transform.position);
            _localRotationOffsetWhileTakingAuthority = Quaternion.Inverse(newGrabber.transform.rotation) * transform.rotation;
            _grabberWhileTakingAuthority = newGrabber;

            // Ask and wait to receive the stateAuthority to move the object
            isTakingAuthority = true;
            await Object.WaitForStateAuthority();
            isTakingAuthority = false;

            // We waited to have the state authority before setting Networked vars
            _localPositionOffset = _localPositionOffsetWhileTakingAuthority;
            _localRotationOffset = _localRotationOffsetWhileTakingAuthority;

            // Update the CurrentGrabber in order to start following position in the FixedUpdateNetwork
            _currentGrabber = _grabberWhileTakingAuthority;
        }

        void DidGrab()
        {
            // While grabbed, we disable physics forces on the object, to force a position based tracking
      
            if (onDidGrab != null) onDidGrab.Invoke(_currentGrabber);
        }

        void DidUngrab()
        {
   

            if (OnDidUngrab != null) OnDidUngrab.Invoke();
        }

        public override void FixedUpdateNetwork()
        {
            // We only update the object position if we have the state authority
            if (!Object.HasStateAuthority) return;

            if (!IsGrabbed) return;
            // Follow grabber, adding position/rotation offsets
            Follow(followingtransform: transform, followedTransform: _currentGrabber.transform, _localPositionOffset, _localRotationOffset);
        }

   

        public override void Render()
        {
            if (isTakingAuthority && _extrapolateWhileTakingAuthority)
            {
                // If we are currently taking the authority on the object due to a grab, the network info are still not set
                //  but we will extrapolate anyway (if the option extrapolateWhileTakingAuthority is true) to avoid having the grabbed object staying still until we receive the authority
                ExtrapolateWhileTakingAuthority();
                return;
            }

            // No need to extrapolate if the object is not grabbed
            if (!IsGrabbed) return;

            // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
            // We extrapolate for all users: we know that the grabbed object should follow accuratly the grabber, even if the network position might be a bit out of sync
            
            Follow(followingtransform: _networkTransform.transform, followedTransform: _currentGrabber.hand.transform, _localPositionOffset, _localRotationOffset);
        }

        void ExtrapolateWhileTakingAuthority()
        {
            // No need to extrapolate if the object is not really grabbed
            if (_grabberWhileTakingAuthority == null) return;

            // Extrapolation: Make visual representation follow grabber, adding position/rotation offsets
            // We use grabberWhileTakingAuthority instead of CurrentGrabber as we are currently waiting for the authority transfer: the network vars are not already set, so we use the temporary versions
            Follow(followingtransform: _networkTransform.transform, followedTransform: _grabberWhileTakingAuthority.hand.transform, _localPositionOffsetWhileTakingAuthority, _localRotationOffsetWhileTakingAuthority);
        }

        void Follow(Transform followingtransform, Transform followedTransform, Vector3 localPositionOffsetToFollowed, Quaternion localRotationOffsetTofollowed)
        {
            followingtransform.position = followedTransform.TransformPoint(localPositionOffsetToFollowed);
            followingtransform.rotation = followedTransform.rotation * localRotationOffsetTofollowed;
        }
    }
}
