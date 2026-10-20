using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(ManiacLogic), "ChaseForPlayer")]
internal static class ML_ChaseForPlayer
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_silent;
	}
}
