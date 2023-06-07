using UnityEditor;

namespace MagicLeapNetworkingDemo.Editor
{

	/// <summary>
	/// Manages define symbol ANDROID_X86_64, ANDROID_ARM7, and ANDROID_ARM64 based on selected Architectures
	/// </summary>
	[InitializeOnLoad]
	public class ProcessorTypeDefines : AssetPostprocessor
	{



		static ProcessorTypeDefines()
		{

			EditorApplication.projectChanged += EditorApplicationOnProjectChanged;
			EditorApplication.delayCall += EditorApplicationOnProjectChanged;
		}


		

		



		private static void EditorApplicationOnProjectChanged()
		{
		
			if (((int)PlayerSettings.Android.targetArchitectures & (~(int)AndroidArchitecture.X86_64)) == 0)
			{
				if (!DefineSymbolUtility.ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64"))
				{
					DefineSymbolUtility.AddDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64");
				}
			}
			else
			{
				if (DefineSymbolUtility.ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64"))
				{
					DefineSymbolUtility.RemoveDefineSymbol(BuildTargetGroup.Android, "ANDROID_X86_64");
				}
			}
			if (((int)PlayerSettings.Android.targetArchitectures & (~(int)AndroidArchitecture.ARMv7)) == 0)
			{
				if (!DefineSymbolUtility.ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7"))
				{
					DefineSymbolUtility.AddDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7");
				}
			}
			else
			{
				if (DefineSymbolUtility.ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7"))
				{
					DefineSymbolUtility.RemoveDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM7");
				}
			}

			if (((int)PlayerSettings.Android.targetArchitectures & (~(int)AndroidArchitecture.ARM64)) == 0)
			{
				if (!DefineSymbolUtility.ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64"))
				{
					DefineSymbolUtility.AddDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64");
				}
			}
			else
			{
				if (DefineSymbolUtility.ContainsDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64"))
				{
					DefineSymbolUtility.RemoveDefineSymbol(BuildTargetGroup.Android, "ANDROID_ARM64");
				}
			}
			
			
		}

	
		
		





		

		



	}
}
