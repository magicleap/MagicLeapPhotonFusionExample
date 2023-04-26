//#define UNSAFE
using System;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections.LowLevel.Unsafe;


/// <summary>
/// Texture readback as Span with AsyncGPUReadback in a synced fashion
/// </summary>
/// <remarks>
///     <para>
///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link</a>.
///     </para>
/// </remarks>
static class TextureReadback
{

#if UNSAFE
    // Texture readback as Span with AsyncGPUReadback in a synced fashion
    public unsafe static ReadOnlySpan<Color32> AsSpan(this Texture source)
    {
        var req = AsyncGPUReadback.Request(source);

        req.WaitForCompletion();
        if (req.hasError) return ReadOnlySpan<Color32>.Empty;

        var data = req.GetData<Color32>(0);

        var ptr = NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(data);
        return new Span<Color32>(ptr, data.Length);
    }
#else
    public static ReadOnlySpan<Color32> AsSpan(this Texture source)
    {
        var req = AsyncGPUReadback.Request(source);

        req.WaitForCompletion();
        if (req.hasError) return ReadOnlySpan<Color32>.Empty;

        var data = req.GetData<Color32>(0);

        return new ReadOnlySpan<Color32>(data.ToArray());
    }
#endif

}
