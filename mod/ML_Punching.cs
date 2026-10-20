using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(ManiacLogic), "Punching")]
internal static class ML_Punching
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
