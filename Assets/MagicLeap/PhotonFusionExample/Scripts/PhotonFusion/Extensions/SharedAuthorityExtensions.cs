using System.Threading.Tasks;
using Fusion;
using UnityEngine;

namespace MagicLeapNetworkingDemo.Extensions
{
	/// <summary>
	/// Script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
	/// </summary>
	public static class SharedAuthorityExtensions
	{

		/// <summary>
		/// Script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
		/// </summary>
		/// <remarks>
		/// <para>
		///  Does the following:
		/// <list type="bullet">
		///     <item>
		///         <description> Request state authority and wait for it to be received.</description>
		///     </item>
		///     <item>
		///         <description> Relevant in shared topology only.</description>
		///     </item>
		///     </list>
		///     </para>
		/// </remarks>
		public static async Task<bool> WaitForStateAuthority(this NetworkObject o, float maxWaitTime = 8)
		{
			float waitStartTime = Time.time;
			o.RequestStateAuthority();
			while (!o.HasStateAuthority && (Time.time - waitStartTime) < maxWaitTime)
			{
				await System.Threading.Tasks.Task.Delay(1);
			}

			return o.HasStateAuthority;
		}
	}
}
