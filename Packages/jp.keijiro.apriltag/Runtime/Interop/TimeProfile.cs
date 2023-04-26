using System;
using System.Runtime.InteropServices;

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
    [StructLayoutAttribute(LayoutKind.Sequential)]
	public unsafe struct TimeProfileEntry
	{
	#region Internal data structure

		private fixed byte name[32];

	#endregion

	#region Public accessors

		public string Name => ConvertName();
		public long UTime { get; }

	#endregion

	#region Internal method

		private string ConvertName()
		{
			fixed (byte* p = name)
			{
				return Marshal.PtrToStringAnsi((IntPtr)p);
			}
		}

	#endregion
	}

	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct TimeProfile
	{
	#region Internal data structure

		private readonly IntPtr stamps;

	#endregion

	#region Public accessors

		public long UTime { get; }

		public unsafe Span<TimeProfileEntry> Stamps => ((ZArray<TimeProfileEntry>*)stamps)->AsSpan;

	#endregion
	}
} // namespace AprilTag.Interop
