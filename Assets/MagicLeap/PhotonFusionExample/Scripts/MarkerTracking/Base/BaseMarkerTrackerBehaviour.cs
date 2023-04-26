using System.Collections.Generic;
using UnityEngine;

namespace MagicLeapNetworkingDemo.MarkerTracking
{
	/// <summary>
	/// Base class for the marker trackers. This allows us to reuse the <see cref="VirtualFiducialMarker"/> script regardless of the API
	/// </summary>
	public abstract class BaseMarkerTrackerBehaviour : MonoBehaviour
	{
		public static BaseMarkerTrackerBehaviour Instance
		{
			get
			{
				var markerTrackers = UnityEngine.Object.FindObjectsOfType(typeof(BaseMarkerTrackerBehaviour), true) as BaseMarkerTrackerBehaviour[];
			
				if (markerTrackers != null)
				{
					foreach (var markerTracker in markerTrackers)
					{
						if (markerTracker.SupportedOnPlatform)
						{
							return markerTracker;
						}
					}
				}

				return null;
			}
		}

		public abstract void StartTracking();
		public abstract void StopTracking();
		
		/// <summary>
		/// Return true if the marker tracker will be active. This insures that BaseMarkerTrackerBehaviour.Instance returns the current tracker
		/// </summary>
		public abstract bool SupportedOnPlatform { get; }

		public abstract MarkerTrackerActions GetActionsForId(int id);
	}
}
