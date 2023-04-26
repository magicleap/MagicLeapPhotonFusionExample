using System;

namespace MagicLeapNetworkingDemo.MarkerTracking
{
		/// <summary>
		/// Class that hold a collection of actions for a marker. These actions are called by the tracker
		/// </summary>
		public class MarkerTrackerActions
		{
			public Action<MarkerPose> Updated;
			public Action Removed;
			public Action Added;
		}
}
