using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(Interactive), "Use")]
internal static class Interactive_Use
{
	[HarmonyPrefix]
	private static bool Prefix(Interactive __instance)
	{

		if (Main.cheat_notrap)
		{
			return (int)__instance.MethodUse != 17;
		}
		return true;
	}
}
