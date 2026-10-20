using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(Noise), "MakeNoise")]
internal static class Noise_MakeNoise
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.cheat_silent;
	}
}
