using Unity.Collections.LowLevel.Unsafe;

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
      internal static class Util
	{
		public static unsafe ref T AsRef<T>(void* source) where T : unmanaged
#if UNITY_2019_1_OR_NEWER
			=> ref UnsafeUtility.AsRef<T>(source);
#else
      => ref System.Runtime.CompilerServices.Unsafe.AsRef<T>(source);
#endif

		public static unsafe void* AsPointer<T>(ref T value) where T : unmanaged
#if UNITY_2019_1_OR_NEWER
			=> UnsafeUtility.AddressOf(ref value);
#else
      => System.Runtime.CompilerServices.Unsafe.AsPointer(ref value);
#endif
	}
} // namespace AprilTag.Interop
