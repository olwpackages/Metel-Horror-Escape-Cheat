using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "PermaDeath")]
internal static class PM_PermaDeath
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
