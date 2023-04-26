using Fusion;
using MagicLeapNetworkingDemo.Input;
using UnityEngine;

namespace MagicLeap.Networking
{

	/// <summary>
	/// Script that is attached to movable objects.
	/// </summary>
	/// <remarks>
	/// <para>
	///   Script does the following:
	/// <list type="bullet">
	///     <item>
	///         <description> Sets the rotation and position of the object across the network. </description>
	///     </item>
	///     <item>
	///         <description> calculates the position for each user based on the shared reference point (Marker). </description>
	///     </item>
	///     </list>
	///     </para>
	/// </remarks>
	public class ColocationObject : NetworkBehaviour
	{
		
		private SharedReferencePoint _referencePoint;
		private Interpolator<Vector3> _positionInterpolator;
		private Interpolator<Quaternion> _rotationInterpolator;
		[Networked]
		public Vector3 Position { get; set; }
		[Networked]
		public Quaternion Rotation { get; set; }
		//StateAuthority means we can control the object.
		public virtual bool HasControl => Object.HasStateAuthority;

		private void Start()
		{
			_referencePoint = SharedReferencePoint.Instance;
			
		}

		public override void Spawned()
		{
			base.Spawned();
			_referencePoint = SharedReferencePoint.Instance;

			_positionInterpolator = GetInterpolator<Vector3>(nameof(Position));
			_rotationInterpolator = GetInterpolator<Quaternion>(nameof(Rotation));
			Position = _referencePoint.transform.InverseTransformPoint(transform.position);
			Rotation = Quaternion.Inverse(_referencePoint.transform.rotation) * transform.rotation;
		
		}
		
		public override void FixedUpdateNetwork()
		{
			// If we can control the object
			if (HasControl)
			{
				Position = _referencePoint.transform.InverseTransformPoint(transform.position);
				Rotation = Quaternion.Inverse(_referencePoint.transform.rotation) * transform.rotation;
			}
	
	
		}

		private void ApplyRelativeWorldPosition(Vector3 position, Quaternion rotation)
		{
			transform.position = _referencePoint.transform.TransformPoint(position);
			transform.rotation = _referencePoint.transform.rotation * rotation;

		}



		public override void Render()
		{
			if (!HasControl)
			{
				ApplyRelativeWorldPosition(_positionInterpolator.Value, _rotationInterpolator.Value);
			}

		
		}
	}
}
