using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "Electricity")]
internal static class PM_Electricity
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
