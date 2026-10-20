using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "Gas")]
internal static class PM_Gas
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
