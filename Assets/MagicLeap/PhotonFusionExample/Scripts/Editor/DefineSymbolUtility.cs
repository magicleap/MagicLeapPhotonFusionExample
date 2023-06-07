using System;
using System.Linq;
using UnityEditor;

namespace MagicLeapNetworkingDemo.Editor
{
    public static class DefineSymbolUtility
    {
        public static void RemoveDefineSymbol(string symbol)
        {
            var targetGroups = GetNonObsoleteTargetGroups();
            foreach (var targetGroup in targetGroups)
            {
                RemoveDefineSymbol(targetGroup, symbol);
            }
        }

        public static void AddDefineSymbol(string symbol)
        {
            var targetGroups = GetNonObsoleteTargetGroups();
            foreach (var targetGroup in targetGroups)
            {
                AddDefineSymbol(targetGroup, symbol);
            }
        }

        public static void RemoveDefineSymbol(BuildTargetGroup targetGroup, string symbol)
        {
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            string newSymbols = string.Join(";", currentSymbols.Split(';').Where(s => s != symbol));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, newSymbols);
        }

        public static void AddDefineSymbol(BuildTargetGroup targetGroup, string symbol)
        {
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            if (!currentSymbols.Split(';').Contains(symbol))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, currentSymbols + ";" + symbol);
            }
        }

        public static bool ContainsDefineSymbolInAnyBuildTarget(string symbol)
        {
            var targetGroups = GetNonObsoleteTargetGroups();
            return targetGroups.Any(targetGroup => ContainsDefineSymbol(targetGroup, symbol));
        }

        public static bool ContainsDefineSymbol(BuildTargetGroup targetGroup, string symbol)
        {
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            return currentSymbols.Split(';').Contains(symbol);
        }

        public static bool ContainsDefineSymbolInAllBuildTargets(string symbol)
        {
            var targetGroups = GetNonObsoleteTargetGroups();
            return targetGroups.All(targetGroup => ContainsDefineSymbol(targetGroup, symbol));
        }

        private static BuildTargetGroup[] GetNonObsoleteTargetGroups()
        {
            var targetGroups = (BuildTargetGroup[]) Enum.GetValues(typeof(BuildTargetGroup));
            return targetGroups.Where(targetGroup => !IsObsolete(targetGroup)).ToArray();
        }

        private static bool IsObsolete(BuildTargetGroup targetGroup)
        {
            return targetGroup == BuildTargetGroup.Unknown || Attribute.IsDefined(typeof(BuildTargetGroup).GetField(targetGroup.ToString()), typeof(ObsoleteAttribute));
        }
    }
}
