using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "GasPermaDeath")]
internal static class PM_GasPermaDeath
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
