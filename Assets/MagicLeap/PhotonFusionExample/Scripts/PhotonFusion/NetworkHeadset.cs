using System.Collections;
using System.Collections.Generic;
using Fusion;
using MagicLeapNetworkingDemo;
using UnityEngine;


namespace MagicLeap.Networking
{
	/// <summary>
	/// Modified version of script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Used to find reference to the network player head transform (the camera).
	/// </para>
	/// </remarks>
	[OrderAfter(typeof(NetworkRig), typeof(NetworkRigidbody))]
	public class NetworkHeadset : NetworkBehaviour
	{
	

	}
}