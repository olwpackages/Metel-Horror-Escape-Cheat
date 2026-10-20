

using HarmonyLib;
using UnityEngine;

namespace MetelCheat;

[HarmonyPatch(typeof(FP_CameraLook), "Update")]
public static class FP_CameraLook_Update
{
	public static bool Prefix()
	{
		return Cursor.lockState == CursorLockMode.Locked;
	}
}
