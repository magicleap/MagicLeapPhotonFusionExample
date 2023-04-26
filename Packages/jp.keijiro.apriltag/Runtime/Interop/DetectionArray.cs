using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag.Interop
{
	/// <summary>
	/// Structure for passing native data.
	/// </summary>
	/// <remarks>
	///     <para>
	///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
	///     </para>
	/// </remarks>
	public sealed class DetectionArray : SafeHandleZeroOrMinusOneIsInvalid
	{
	#region SafeHandle implementation

		DetectionArray() : base(true) { }

		protected override bool ReleaseHandle()
		{
			_Destroy(handle);
			return true;
		}

	#endregion

	#region zarray representation

		unsafe ref ZArray<IntPtr> AsPointerArray => ref Util.AsRef<ZArray<IntPtr>>((void*)handle);

	#endregion

	#region Public methods

		public int Length => AsPointerArray.AsSpan.Length;

		public unsafe ref Detection this[int i] => ref Util.AsRef<Detection>((void*)AsPointerArray.AsSpan[i]);

	#endregion

	#region Unmanaged interface

		[DllImport(Config.DllName, EntryPoint = "apriltag_detections_destroy")]
		static extern void _Destroy(IntPtr detections);

	#endregion
	}

} // namespace AprilTag.Interop
