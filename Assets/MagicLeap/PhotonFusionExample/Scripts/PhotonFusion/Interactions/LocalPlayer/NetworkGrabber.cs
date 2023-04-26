using Fusion;
using MagicLeap.Networking;
using UnityEngine;

namespace MagicLeapNetworkingDemo.Grabbing
{

    /// <summary>
    /// Modified version of script: from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Networked version of the local Grabber.
    /// </para>
    /// <para>
    /// This networked version is needed to expose the networkTransform to the grabbing logic, to extrapolate properly the visuals (interpolation targets).
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(NetworkGameController))]
    [OrderAfter(typeof(NetworkGameController))]
    public class NetworkGrabber : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkGameController Hand;
        public override void Spawned()
        {
            base.Spawned();
            Hand = GetComponentInParent<NetworkGameController>();
            if (Hand.IsLocalNetworkRig)
            {
                // References itself in its local counterpart, to simplify the lookup during local grabbing
                if (Hand.LocalHardwareGameController)
                {
                    Grabber grabber = Hand.LocalHardwareGameController.GetComponentInChildren<Grabber>();
                    grabber.networkGrabber = this;
                }
            }
        }
    }
}
