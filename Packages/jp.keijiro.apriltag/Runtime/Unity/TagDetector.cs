using System;
using System.Collections.Generic;
using AprilTag.Interop;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace AprilTag
{
    /// <summary>
    /// Multithreaded tag detector and pose estimator.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
    ///     </para>
    /// </remarks>
    public sealed class TagDetector : IDisposable
	{
	#region Constructor

		public TagDetector(int width, int height, int decimation = 2, Family.TagType tagTypes = Family.TagType.TagCustom48h12)
		{
			trackedTagFamilies = new List<Family>();
			_detector = Detector.Create();
			_image = ImageU8.Create(width, height);
			// Detector configuration
			_detector.ThreadCount = SystemConfig.PreferredThreadCount;
			_detector.QuadDecimate = decimation;
			foreach (Family.TagType value in Enum.GetValues(tagTypes.GetType()))
			{
				if (tagTypes.HasFlag(value))
				{
					var family = Family.CreateTag(value);
					trackedTagFamilies.Add(family);
					_detector.AddFamily(family);
				}
			}

			IsDisposed = false;
		}

	#endregion

	#region Detection/estimation procedure

		//
		// We can simply use the multithreaded AprilTag detector for tag detection.
		//
		// In contrast, AprilTag only provides single-threaded pose estimator, so
		// we have to manage threading ourselves.
		//
		// We don't want to spawn extra threads just for it, so we run them on
		// Unity's job system. It's a bit complicated due to "impedance mismatch"
		// things (unmanaged vs managed vs Unity DOTS).
		//
		private void RunDetectorAndEstimator(float fov, float tagSize)
		{
			_profileData = null;

			// Run the AprilTag detector.
			using var tags = _detector.Detect(_image);
			var tagCount = tags.Length;

			// Convert the detector output into a NativeArray to make them
			// accessible from the pose estimation job.
			using var jobInput = new NativeArray<PoseEstimationJob.Input>(tagCount, Allocator.TempJob);

			var slice = new NativeSlice<PoseEstimationJob.Input>(jobInput);

			for (var i = 0; i < tagCount; i++)
			{
				slice[i] = new PoseEstimationJob.Input(ref tags[i]);
			}

			// Pose estimation output buffer
			using var jobOutput
				= new NativeArray<TagPose>(tagCount, Allocator.TempJob);

			// Pose estimation job
			var job = new PoseEstimationJob(jobInput, jobOutput, _image.Width, _image.Height, fov, tagSize);

			// Run and wait the jobs.
			job.Schedule(tagCount, 1).Complete();

			// Job output -> managed list
			jobOutput.CopyTo(_detectedTags);
		}

	#endregion

	#region Profile data aggregation

		private List<(string, long)> GenerateProfileData()
		{
			var list = new List<(string, long)>();
			var stamps = _detector.TimeProfile.Stamps;
			var time = _detector.TimeProfile.UTime;
			for (var i = 0; i < stamps.Length; i++)
			{
				var stamp = stamps[i];
				list.Add((stamp.Name, stamp.UTime - time));
				time = stamp.UTime;
			}

			return list;
		}

	#endregion

	#region Public properties

		public IEnumerable<TagPose> DetectedTags => _detectedTags;

		public IEnumerable<(string name, long time)> ProfileData => _profileData ?? (_profileData = GenerateProfileData());

		private readonly List<Family> trackedTagFamilies;
		public bool IsDisposed { get; private set; }

	#endregion

	#region Public methods

		public void Dispose()
		{
			foreach (var family in trackedTagFamilies)
			{
				_detector?.RemoveFamily(family);
				family?.Dispose();
			}

			trackedTagFamilies.Clear();

			_detector?.Dispose();
			_image?.Dispose();

			_detector = null;
			_image = null;
			IsDisposed = true;
		}

		public void ProcessImage(ReadOnlySpan<Color32> image, float fov, float tagSize)
		{
			ImageConverter.Convert(image, _image);

			RunDetectorAndEstimator(fov, tagSize);
		}

	#endregion

	#region Private objects

		private Detector _detector;

		private ImageU8 _image;

		private readonly List<TagPose> _detectedTags = new();
		private List<(string, long)> _profileData;

	#endregion
	}
} // namespace AprilTag
