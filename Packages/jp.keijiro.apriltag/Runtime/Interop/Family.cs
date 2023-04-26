using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace AprilTag.Interop
{
	/// <summary>
	/// ImageU8 image conversion.
	/// </summary>
	/// <remarks>
	///     <para>
	///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
	///     </para>
	///     <para>
	///     Edited to support more marker types.
	///     </para>
	/// </remarks>
	public sealed class Family : SafeHandleZeroOrMinusOneIsInvalid
	{
	#region SafeHandle implementation

		public TagType tag;
		private Family() : base(true) { }

		protected override bool ReleaseHandle()
		{
			switch (tag)
			{
				case TagType.TagStandard41h12:
					_DestroyTagStandard41h12(handle);
					break;
				case TagType.Tag36h11:
					_DestroyTag36h11(handle);
					break;
				case TagType.Tag16h5:
					_DestroyTag16h5(handle);
					break;
				case TagType.Tag25h9:
					_DestroyTag25h9(handle);
					break;
				case TagType.TagCircle21h7:
					_DestroyTagCircle21h7(handle);
					break;
				case TagType.TagCircle49h12:
					_DestroyTagCircle49h12(handle);
					break;
				case TagType.TagCustom48h12:
					_DestroyTagCustom48h12(handle);
					break;
				case TagType.TagStandard52h13:
					_DestroyTagStandard52h13(handle);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return true;
		}

	#endregion

	#region Public methods

		[Flags]
		public enum TagType
		{
			Tag16h5 = 1 << 0,
			Tag25h9 = 1 << 1,
			Tag36h11 = 1 << 2,
			TagCircle21h7 = 1 << 3,
			TagCircle49h12 = 1 << 4,
			TagCustom48h12 = 1 << 5,
			TagStandard41h12 = 1 << 6,
			TagStandard52h13 = 1 << 7
		}


		public static Family CreateTag(TagType tagType)
		{
			Family returnValue = default;
			switch (tagType)
			{
				case TagType.TagStandard41h12:
					returnValue = _CreateTagStandard41h12();
					break;
				case TagType.Tag36h11:
					returnValue = _CreateTag36h11();
					break;
				case TagType.Tag16h5:
					returnValue = _CreateTag16h5();
					break;
				case TagType.Tag25h9:
					returnValue = _CreateTag25h9();
					break;
				case TagType.TagCircle21h7:
					returnValue = _CreateTagCircle21h7();
					break;
				case TagType.TagCircle49h12:
					returnValue = _CreateTagCircle49h12();
					break;
				case TagType.TagCustom48h12:
					returnValue = _CreateTagCustom48h12();
					break;
				case TagType.TagStandard52h13:
					returnValue = _CreateTagStandard52h13();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			returnValue.tag = tagType;
			return returnValue;
		}

	#endregion

	#region Unmanaged interface

		[DllImport(Config.DllName, EntryPoint = "tag16h5_create")]
		private static extern Family _CreateTag16h5();

		[DllImport(Config.DllName, EntryPoint = "tag16h5_destroy")]
		private static extern void _DestroyTag16h5(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tag25h9_create")]
		private static extern Family _CreateTag25h9();

		[DllImport(Config.DllName, EntryPoint = "tag25h9_destroy")]
		private static extern void _DestroyTag25h9(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tag36h11_create")]
		private static extern Family _CreateTag36h11();

		[DllImport(Config.DllName, EntryPoint = "tag36h11_destroy")]
		private static extern void _DestroyTag36h11(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tagCircle21h7_create")]
		private static extern Family _CreateTagCircle21h7();

		[DllImport(Config.DllName, EntryPoint = "tagCircle21h7_destroy")]
		private static extern void _DestroyTagCircle21h7(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tagCircle49h12_create")]
		private static extern Family _CreateTagCircle49h12();

		[DllImport(Config.DllName, EntryPoint = "tagCircle49h12_destroy")]
		private static extern void _DestroyTagCircle49h12(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tagCustom48h12_create")]
		private static extern Family _CreateTagCustom48h12();

		[DllImport(Config.DllName, EntryPoint = "tagCustom48h12_destroy")]
		private static extern void _DestroyTagCustom48h12(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tagStandard41h12_create")]
		private static extern Family _CreateTagStandard41h12();

		[DllImport(Config.DllName, EntryPoint = "tagStandard41h12_destroy")]
		private static extern void _DestroyTagStandard41h12(IntPtr ptr);

		[DllImport(Config.DllName, EntryPoint = "tagStandard52h13_create")]
		private static extern Family _CreateTagStandard52h13();

		[DllImport(Config.DllName, EntryPoint = "tagStandard52h13_destroy")]
		private static extern void _DestroyTagStandard52h13(IntPtr ptr);

	#endregion
	}
} // namespace AprilTag.Interop
