namespace AprilTag.Interop
{
    /// <summary>
    /// DllName configuration.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Credit: Keijiro Takahashi's April Tags for Unity: <a href="https://github.com/keijiro/jp.keijiro.apriltag"> Github Repository Link </a>.
    ///     </para>
    /// </remarks>
    internal static class Config
	{
#if UNITY_EDITOR || !UNITY_IOS
		public const string DllName = "AprilTag";
#else
    public const string DllName = "__Internal";
#endif
	}
} // namespace AprilTag.Interop
