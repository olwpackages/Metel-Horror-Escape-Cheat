using HarmonyLib;
using LinkedSquad.AI.Movement;

namespace MetelCheat;

[HarmonyPatch(typeof(AgentMovement), "MoveTo")]
internal static class AgentMovement_MoveTo
{
	[HarmonyPrefix]
	private static bool Prefix()
	{
		return !Main.FreezeActive;
	}
}
