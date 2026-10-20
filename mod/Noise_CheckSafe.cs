using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(Noise), "CheckSafe")]
internal static class Noise_CheckSafe
{
	[HarmonyPostfix]
	private static void Postfix(ref bool __result)
	{
		if (Main.cheat_silent)
		{
			__result = true;
		}
	}
}
