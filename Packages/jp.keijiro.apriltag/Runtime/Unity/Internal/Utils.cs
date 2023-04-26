using System.Collections.Generic;
using AprilTag.Interop;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;


/// <summary>
/// Small utilities and extension methods.
/// </summary>
/// <remarks>
///     <para>
///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link</a>.
///     </para>
/// </remarks>
namespace AprilTag
{
	internal static class SystemConfig
	{
		public static int PreferredThreadCount => math.max(1, JobsUtility.JobWorkerCount);
	}

	internal static class NativeArrayExtensions
	{
		public static void
			CopyTo<T>(this NativeArray<T> array, List<T> list) where T : unmanaged
		{
			list.Clear();
			list.Capacity = array.Length;
			for (var i = 0; i < array.Length; i++)
			{
				list.Add(array[i]);
			}
		}
	}

	internal static class MatdExtensions
	{
		public static float3 AsFloat3(this ref Matd3x1 src)
		{
			return math.float3((float)src.e0, (float)src.e1, (float)src.e2);
		}

		public static float3x3 AsFloat3x3(this ref Matd3x3 src)
		{
			return math.float3x3((float)src.e00, (float)src.e01, (float)src.e02,
								(float)src.e10, (float)src.e11, (float)src.e12,
								(float)src.e20, (float)src.e21, (float)src.e22);
		}
	}
} // namespace AprilTag
