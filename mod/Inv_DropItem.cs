using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(Inventory), "DropItem")]
internal static class Inv_DropItem
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_keepitems;
	}
}
