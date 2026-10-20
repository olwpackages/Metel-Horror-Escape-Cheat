using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(ManiacLogic), "KillInHidePlace")]
internal static class ML_KillInHidePlace
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
