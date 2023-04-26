using UnityEngine;

namespace MagicLeapNetworkingDemo.MarkerTracking
{
	/// <summary>
	/// Generic class to hold and consume the marker position and rotation
	/// </summary>
	public class MarkerPose
	{
		public readonly uint Id;
		public Vector3 Position;
		public Quaternion Rotation;

		public MarkerPose(uint id, Vector3 position, Quaternion rotation)
		{
			Id = id;
			Position = position;
			Rotation = rotation;
		}
	}
}
