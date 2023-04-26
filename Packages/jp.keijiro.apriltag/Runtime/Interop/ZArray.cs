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
	public struct ZArray<T> where T : unmanaged
	{
	#region Internal data structure

		private readonly ulong el_sz;
		private readonly int size;
		private readonly int alloc;
		private readonly IntPtr data;

	#endregion

	#region Public accessors

		public unsafe Span<T> AsSpan => new((void*)data, size);

	#endregion
	}
} // namespace AprilTag.Interop
