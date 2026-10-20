using HarmonyLib;

namespace MetelCheat;

[HarmonyPatch(typeof(TriggerSpace), "Trigger")]
internal static class TriggerSpace_Trigger
{
	[HarmonyPrefix]
	private static bool Prefix(TriggerSpace __instance)
	{
		if (!Main.cheat_notrap)
		{
			return true;
		}
		var mt = (TriggerSpace.MethodTriggerEnum)Traverse.Create(__instance).Field("MethodTrigger").GetValue();
		return mt != TriggerSpace.MethodTriggerEnum.Trap;
	}
}
