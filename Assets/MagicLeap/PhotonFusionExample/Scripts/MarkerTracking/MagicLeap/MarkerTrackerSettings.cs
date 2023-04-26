using UnityEngine;
using UnityEngine.XR.MagicLeap;

namespace MagicLeapNetworkingDemo.MarkerTracking
{
	/// <summary>
	/// Settings used by <see cref="MagicLeapMarkerTracker"/> to start the marker tracker.
	/// </summary>
	///<param name="TrackerProfile">Setting this to <see cref="MLMarkerTracker.Profile.Default" /> will ignore the profile settings set in this Scriptable Object</param>
	[CreateAssetMenu(fileName = "MarkerTrackerSettings", menuName = "Marker Tracker/Scriptable Objects/New Settings", order = 0)]
	public class MarkerTrackerSettings : ScriptableObject
	{

		/// <summary>
		/// The marker types that are enabled for this scanner. Enable markers by
		/// combining any number of <c> MarkerType </c> flags using '|' (bitwise 'or').
		/// </summary>
		[Header("Marker Detection Settings")]
		public MLMarkerTracker.MarkerType MarkerTypes = MLMarkerTracker.MarkerType.All;

		/// <summary>
		/// Represents the different tracker profiles used to optimize marker tracking in difference use cases.
		/// </summary>
		public MLMarkerTracker.Profile TrackerProfile = MLMarkerTracker.Profile.Custom;
		
		/// <summary>
		///     Aruco marker size to use (in meters).
		/// </summary>
		public float ArucoMarkerSize = 0.1f;

		/// <summary>
		///     The physical size of the QR code that shall be tracked (in meters). The physical size is
		///     important to know, because once a QR code is detected we can only determine its
		///     3D position when we know its correct size. The size of the QR code is given in
		///     meters and represents the length of one side of the square code(without the
		///     outer margin). Min size: As a rule of thumb the size of a QR code should be at
		///     least a 10th of the distance you intend to scan it with a camera device. Higher
		///     version markers with higher information density might need to be larger than
		///     that to be detected reliably. Max size: Our camera needs to see the whole
		///     marker at once. If it's too large, we won't detect it.
		/// </summary>
		public float QRCodeSize = 0.1f;

		/// <summary>
		///     Aruco dictionary to use.
		/// </summary>

		public MLMarkerTracker.ArucoDictionaryName ArucoDicitonary;
		
		
		/// <summary>
		///     A hint to the back-end the max frames per second hat should be analyzed.
		/// </summary>
		[Space,Header("Profile Settings")]
		public MLMarkerTracker.FPSHint FPSHint;

		/// <summary>
		///     A hint to the back-end the resolution that should be used.
		/// </summary>
		public MLMarkerTracker.ResolutionHint ResolutionHint;

		/// <summary>
		///     In order to improve performance, the detectors don't always run on the full
		///     frame.Full frame analysis is however necessary to detect new markers that
		///     weren't detected before. Use this option to control how often the detector may
		///     detect new markers and its impact on tracking performance.
		/// </summary>
		public MLMarkerTracker.FullAnalysisIntervalHint FullAnalysisIntervalHint;

		public MLMarkerTracker.CameraHint CameraHint = MLMarkerTracker.CameraHint.RGB;

		/// <summary>
		///     This option provides control over corner refinement methods and a way to
		///     balance detection rate, speed and pose accuracy. Always available and
		///     applicable for Aruco and April tags.
		/// </summary>
		public MLMarkerTracker.CornerRefineMethod CornerRefineMethod;

		/// <summary>
		///     Run refinement step that uses marker edges to generate even more accurate
		///     corners, but slow down tracking rate overall by consuming more compute.
		///     Aruco/April tags only.
		/// </summary>

		public bool UseEdgeRefinement;



	}
}
