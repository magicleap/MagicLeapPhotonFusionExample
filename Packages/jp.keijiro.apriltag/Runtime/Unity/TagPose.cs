using UnityEngine;

namespace AprilTag
{
    /// <summary>
    /// Tag pose structure for storing an estimated pose.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
    ///     </para>
    /// </remarks>
    public struct TagPose
	{
		public int ID { get; }
		public Vector3 Position { get; }
		public Quaternion Rotation { get; }

		public TagPose(int id, Vector3 position, Quaternion rotation)
		{
			ID = id;
			Position = position;
			Rotation = rotation;
		}
	}
} // namespace AprilTag
