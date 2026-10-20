using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "Damage")]
internal static class PM_Damage
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
