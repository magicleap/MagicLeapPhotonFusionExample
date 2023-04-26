using UnityEngine;

namespace MagicLeapNetworkingDemo.Extensions
{
	/// <summary>
	/// Script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
	/// </summary>
	public static class VelocityExtension
	{
		/// <summary>
		/// Script from:<a href="https://doc.photonengine.com/fusion/current/technical-samples/fusion-vr-shared"> Photon Fusion Technical Samples - VR Shared </a>
		/// </summary>
		/// <remarks>
		/// <para>
		/// Gets the angular velocity based on previous rotation, current rotation, and time passed.
		/// </para>
		/// </remarks>
		public static Vector3 AngularVelocityChange(this Quaternion previousRotation, Quaternion newRotation, float elapsedTime)
		{
			Quaternion rotationStep = newRotation * Quaternion.Inverse(previousRotation);
			rotationStep.ToAngleAxis(out float angle, out Vector3 axis);
			// Angular velocity uses eurler notation, bound to -180� / +180�
			if (angle > 180f)
			{
				angle -= 360f;
			}

			if (Mathf.Abs(angle) > Mathf.Epsilon)
			{
				float radAngle = angle * Mathf.Deg2Rad;
				Vector3 angularStep = axis * radAngle;
				Vector3 angularVelocity = angularStep / elapsedTime;
				if (!float.IsNaN(angularVelocity.x))
					return angularVelocity;
			}

			return Vector3.zero;
		}
	}
}
