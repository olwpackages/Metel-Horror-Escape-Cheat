using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "Dead")]
internal static class PM_Dead
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
