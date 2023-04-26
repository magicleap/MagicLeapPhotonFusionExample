using Fusion;

namespace MagicLeapNetworkingDemo.Input
{
	/// <summary>
	/// Script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Input used for grabbing objects. 
	/// </para>
	/// </remarks>
	public struct ControllerInputData : INetworkStruct
	{

		public float gripCommand;
		public float triggerCommand;

	}
}
