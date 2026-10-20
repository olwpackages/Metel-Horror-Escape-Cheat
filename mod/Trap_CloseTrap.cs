using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(Trap), "CloseTrap")]
internal static class Trap_CloseTrap
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_notrap;
	}
}
