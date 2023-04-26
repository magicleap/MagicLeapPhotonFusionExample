using System;
using System.Runtime.InteropServices;

namespace AprilTag.Interop
{
	/// <summary>
	/// April Tag detector info.
	/// </summary>
	/// <remarks>
	///     <para>
	///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
	///     </para>
	/// </remarks>
	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct DetectionInfo
	{
	#region Internal data structure

		IntPtr det;
		double tagsize;
		double fx, fy;
		double cx, cy;

	#endregion

	#region Constructor

		unsafe public DetectionInfo(ref Detection detection, double tagSize,
									double fx, double fy, double cx, double cy)
		{
			this.det = (IntPtr)Util.AsPointer(ref detection);
			this.tagsize = tagSize;
			this.fx = fx;
			this.fy = fy;
			this.cx = cx;
			this.cy = cy;
		}

	#endregion
	}

} // namespace AprilTag.Interop
