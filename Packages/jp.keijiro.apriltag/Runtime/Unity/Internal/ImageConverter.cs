using System;
using AprilTag.Interop;
using Unity.Burst;
using UnityEngine;

namespace AprilTag
{
    /// <summary>
    /// Burst-accelerated image convertion functions.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
    ///     </para>
    /// </remarks>
    [BurstCompile]
	internal static class ImageConverter
	{
		public static unsafe void
			Convert(ReadOnlySpan<Color32> data, ImageU8 image)
		{
			fixed (Color32* src = &data.GetPinnableReference())
			fixed (byte* dst = &image.Buffer.GetPinnableReference())
			{
				BurstConvert(src, dst, image.Width, image.Height, image.Stride);
			}
		}

		[BurstCompile]
		private static unsafe void BurstConvert(Color32* src, byte* dst, int width, int height, int stride)
		{
			var offs_src = 0;
			var offs_dst = stride * (height - 1);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					dst[offs_dst + x] = src[offs_src + x].g;
				}

				offs_src += width;
				offs_dst -= stride;
			}
		}
	}
} // namespace AprilTag
