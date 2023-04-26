using System.Collections.Generic;
using Fusion;
using MagicLeapNetworkingDemo;
using MagicLeapNetworkingDemo.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace MagicLeap.Networking
{
    /// <summary>
    /// Modified version of script "NetworkHand": from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use the local <see cref="HardwareRig"/> controller position when this rig is associated with the local user. 
    /// </para>
    /// <para>
    /// Position synchronization is handled in the <see cref="NetworkRig"/>.
    /// </para>
    /// </remarks>
    [OrderAfter(typeof(NetworkRig), typeof(NetworkRigidbody))]
    public class NetworkGameController : NetworkBehaviour
    {

      
        public ControllerInputData controllerInputData { get; set; }
        private NetworkRig _networkRig;

        public bool IsLocalNetworkRig => _networkRig!= null && _networkRig.HasControl;

        public HardwareGameController LocalHardwareGameController => IsLocalNetworkRig ? _networkRig.HardwareRig.gameController : null;

        
        private void Awake()
        {
            _networkRig = GetComponentInParent<NetworkRig>();
        }
     
    }
}
