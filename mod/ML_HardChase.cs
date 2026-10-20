using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(ManiacLogic), "HardChase")]
internal static class ML_HardChase
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_silent;
	}
}
