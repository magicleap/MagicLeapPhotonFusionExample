using System;
using System.Runtime.InteropServices;

namespace AprilTag.Interop
{
    /// <summary>
    /// Structure for getting the pose of the marker.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
    ///     </para>
    /// </remarks>
    [StructLayoutAttribute(LayoutKind.Sequential)]
	public struct Pose : IDisposable
	{
	#region Internal data structure

		private IntPtr matd_r;
		private IntPtr matd_t;

	#endregion

	#region Public properties and methods

		public unsafe ref Matd3x3 R => ref Util.AsRef<Matd3x3>((void*)matd_r);
		public unsafe ref Matd3x1 t => ref Util.AsRef<Matd3x1>((void*)matd_t);

		public Pose(ref DetectionInfo info)
		{
			matd_r = matd_t = IntPtr.Zero;
			_Estimate(ref info, ref this);
		}

		public void Dispose()
		{
			if (matd_r != IntPtr.Zero)
			{
				_MatdDestroy(matd_r);
			}

			if (matd_t != IntPtr.Zero)
			{
				_MatdDestroy(matd_t);
			}

			matd_r = matd_t = IntPtr.Zero;
		}

	#endregion

	#region Unmanaged interface

		[DllImport(Config.DllName, EntryPoint = "matd_destroy")]
		private static extern void _MatdDestroy(IntPtr matd);

		[DllImport(Config.DllName, EntryPoint = "estimate_tag_pose")]
		private static extern double _Estimate(ref DetectionInfo info, ref Pose pose);

	#endregion
	}
} // namespace AprilTag.Interop
