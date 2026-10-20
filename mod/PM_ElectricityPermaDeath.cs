using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(PlayerManager), "ElectricityPermaDeath")]
internal static class PM_ElectricityPermaDeath
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_god;
	}
}
