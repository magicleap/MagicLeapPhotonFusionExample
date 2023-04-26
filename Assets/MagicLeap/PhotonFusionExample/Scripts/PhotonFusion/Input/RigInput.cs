using Fusion;
using UnityEngine;

namespace MagicLeapNetworkingDemo.Input
{
	/// <summary>
	/// Script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Input that represents the users rig.
	/// </para>
	/// <para>
	/// This data is collected by  <see cref="HardwareRig"/> and consumed by <see cref="MagicLeap.Networking.NetworkRig"/>.
	/// </para>
	/// </remarks>
	public struct RigInput : INetworkInput
	{
		public Vector3 CenterPosition;
		public Quaternion CenterRotation;
		public Vector3 ControllerPosition;
		public Quaternion ControllerRotation;
		public Vector3 headsetPosition;
		public Quaternion headsetRotation;
		public ControllerInputData ControllerInput;

	}
}
