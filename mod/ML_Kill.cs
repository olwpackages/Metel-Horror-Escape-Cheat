using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(ManiacLogic), "Kill")]
internal static class ML_Kill
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
