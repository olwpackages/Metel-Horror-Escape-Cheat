using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(ManiacLogic), "Damage")]
internal static class ML_Damage
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
