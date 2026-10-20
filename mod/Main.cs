using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LinkedSquad.InventorySystem;
using MelonLoader;
using Project.AI.BehaviourTree.Agents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

namespace MetelCheat;

public class Main : MelonMod
{
	private struct FrozenThing
	{
		public NavMeshAgent Agent;

		public float BaseSpeed;

		public Animator Anim;
	}

	public static bool cheat_god;

	public static bool cheat_silent;

	public static bool cheat_fly;

	public static bool cheat_freeze;

	public static bool cheat_keepitems;

	public static bool cheat_esp_maniac;

	public static bool cheat_esp_items;

	public static bool cheat_fullbright;

	public static bool cheat_notrap;

	public static bool cheat_steal;

	public static float cheat_speedmult = 1f;

	public enum CheatAction
	{
		Menu,
		God,
		Silent,
		Fly,
		Freeze,
		KeepItems,
		Bright,
		EspManiac,
		EspItems,
		NotTrap,
		Steal,
		SpeedDown,
		SpeedUp,
		UnlockAll
	}

	private static readonly Dictionary<CheatAction, KeyCode> _binds = new Dictionary<CheatAction, KeyCode>();

	private static KeyCode BindDefault(CheatAction a) => KeyCode.None;

	private static HarmonyLib.Harmony _harmony;

	private static bool _menuOpen;

	private static bool _uiCreated;

	private static FP_Controller _speedOwner;

	private static float _baseWalk;

	private static float _baseRun;

	private static float _baseCrouch;

	private static bool _prevTrapped;

	private static bool _brightSaved;

	private static Color _oldAmbient;

	private static float _oldIntensity;

	private static bool _oldFog;

	private static AmbientMode _oldMode;

	private static GameObject _brightLight;

	private static float _nextUntrap;

	private static float _nextSteal;

	private static bool _wasFrozen;

	private static bool _frozenCacheBuilt;

	private static readonly List<FrozenThing> frozen = new List<FrozenThing>();

	private static readonly List<MonoBehaviour> frozenMovers = new List<MonoBehaviour>();

	public static bool IsMenuOpen => _menuOpen;

	public static void OpenMenu() => _menuOpen = true;

	public static bool FreezeActive
	{
		get
		{
			if (!_wasFrozen)
			{
				if (cheat_freeze)
				{
					if (!((UnityEngine.Object)(object)PlayerManager.instance == (UnityEngine.Object)null))
					{
						return !PlayerManager.instance.IsDead;
					}
					return true;
				}
				return false;
			}
			return true;
		}
	}

	public static string SpeedLabel
	{
		get
		{
			float num = cheat_speedmult;
			if (!(Math.Abs((double)num - Math.Floor(num)) < 0.009999999776482582))
			{
				return num.ToString("0.##");
			}
			return ((int)num).ToString();
		}
	}

	public override void OnInitializeMelon()
	{

		if (PlayerPrefs.GetInt("metelcheat_reset_v12", 0) == 0)
		{
			string[] array = new string[9] { "metelcheat_god", "metelcheat_silent", "metelcheat_freeze", "metelcheat_keepitems", "metelcheat_espm", "metelcheat_espi", "metelcheat_bright", "metelcheat_notrap", "metelcheat_steal" };
			for (int i = 0; i < array.Length; i++)
			{
				PlayerPrefs.SetInt(array[i], 0);
			}
			PlayerPrefs.SetInt("metelcheat_reset_v12", 1);
			PlayerPrefs.Save();
		}
		cheat_god = PlayerPrefs.GetInt("metelcheat_god", 0) == 1;
		cheat_silent = PlayerPrefs.GetInt("metelcheat_silent", 0) == 1;
		cheat_freeze = PlayerPrefs.GetInt("metelcheat_freeze", 0) == 1;
		cheat_keepitems = PlayerPrefs.GetInt("metelcheat_keepitems", 0) == 1;
		cheat_esp_maniac = PlayerPrefs.GetInt("metelcheat_espm", 0) == 1;
		cheat_esp_items = PlayerPrefs.GetInt("metelcheat_espi", 0) == 1;
		cheat_fullbright = PlayerPrefs.GetInt("metelcheat_bright", 0) == 1;
		cheat_notrap = PlayerPrefs.GetInt("metelcheat_notrap", 0) == 1;
		cheat_steal = PlayerPrefs.GetInt("metelcheat_steal", 0) == 1;
		cheat_speedmult = PlayerPrefs.GetFloat("metelcheat_speed", 1f);
		if (cheat_speedmult <= 0.05f)
		{
			cheat_speedmult = 1f;
		}

		if (PlayerPrefs.GetInt("metelcheat_bindreset_v13", 0) == 0)
		{
			foreach (CheatAction a in Enum.GetValues(typeof(CheatAction)))
				PlayerPrefs.SetInt("metelcheat_bind_" + a, (int)KeyCode.None);
			PlayerPrefs.SetInt("metelcheat_bindreset_v13", 1);
			PlayerPrefs.Save();
		}
		LoadBinds();
		_harmony = new HarmonyLib.Harmony("metel.cheat");
		_harmony.PatchAll(typeof(Main).Assembly);
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		int n = 0;
		foreach (System.Reflection.MethodInfo m in _harmony.GetPatchedMethods())
		{
			sb.Append(m.DeclaringType.Name).Append('.').Append(m.Name).AppendLine();
			n++;
		}
		MelonLogger.Msg("Harmony patched methods (" + n + "):\n" + sb);
		MelonLogger.Msg("MetelCheat 1.2 loaded. Bind keys are set in the menu.");
	}

	public override void OnSceneWasLoaded(int buildIndex, string sceneName)
	{

		if (!_uiCreated)
		{
			GameObject val = new GameObject("MetelCheatUi");
			UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)val);
			((UnityEngine.Object)val).hideFlags = (HideFlags)61;
			val.AddComponent<CheatUi>();
			_uiCreated = true;
		}
		frozen.Clear();
		frozenMovers.Clear();
		_frozenCacheBuilt = false;
		_wasFrozen = false;
		CheatUi.self?.ResetStyles();
		CheatUi.self?.Message("Сцена: " + sceneName);
	}

	public override void OnUpdate()
	{
		HandleKeys();
		TickGod();
		TickSilent();
		TickSpeed();
		TickFly();
		TickFreeze();
		TickBright();
		TickNotrap();
		TickSteal();
		if (_menuOpen)
		{
			Cursor.visible = true;
			Cursor.lockState = (CursorLockMode)0;
		}
	}

	private static void TickBright()
	{

		if (cheat_fullbright)
		{
			if (!_brightSaved)
			{
				_oldAmbient = RenderSettings.ambientLight;
				_oldIntensity = RenderSettings.ambientIntensity;
				_oldFog = RenderSettings.fog;
				_oldMode = RenderSettings.ambientMode;
				_brightSaved = true;
			}
			RenderSettings.fog = false;
			RenderSettings.ambientMode = (AmbientMode)3;
			RenderSettings.ambientLight = new Color(0.92f, 0.92f, 0.92f);
			RenderSettings.ambientIntensity = 2.2f;
			if ((UnityEngine.Object)(object)_brightLight == (UnityEngine.Object)null)
			{
				_brightLight = new GameObject("MetelCheatBrightLight");
				UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object)(object)_brightLight);
				((UnityEngine.Object)_brightLight).hideFlags = (HideFlags)61;
				Light val = _brightLight.AddComponent<Light>();
				val.type = (LightType)1;
				val.intensity = 0.85f;
				val.shadows = (LightShadows)0;
			}
			Camera main = Camera.main;
			if ((UnityEngine.Object)(object)main != (UnityEngine.Object)null)
			{
				_brightLight.transform.rotation = ((Component)main).transform.rotation;
			}
		}
		else if (_brightSaved)
		{
			RenderSettings.ambientLight = _oldAmbient;
			RenderSettings.ambientIntensity = _oldIntensity;
			RenderSettings.fog = _oldFog;
			RenderSettings.ambientMode = _oldMode;
			if ((UnityEngine.Object)(object)_brightLight != (UnityEngine.Object)null)
			{
				UnityEngine.Object.Destroy((UnityEngine.Object)(object)_brightLight);
				_brightLight = null;
			}
			_brightSaved = false;
		}
	}

	private static void TickNotrap()
	{
		if (!cheat_notrap)
		{
			return;
		}

		FP_Controller instance = FP_Controller.instance;
		if ((UnityEngine.Object)(object)instance != (UnityEngine.Object)null)
		{
			instance.trapped = instance.trapped && cheat_fly;
		}
		if (Time.unscaledTime < _nextUntrap)
		{
			return;
		}
		_nextUntrap = Time.unscaledTime + 0.5f;
		Interactive[] array = UnityEngine.Object.FindObjectsOfType<Interactive>();
		foreach (Interactive val in array)
		{
			if ((int)val.MethodUse == 17 && val.Used)
			{
				Traverse.Create((object)val).Property("Used", (object[])null).SetValue((object)false);
			}
		}
	}

	private static void TickSteal()
	{
		if (!cheat_steal || Time.unscaledTime < _nextSteal)
		{
			return;
		}
		_nextSteal = Time.unscaledTime + 1.2f;
		List<Transform> leftovers = new List<Transform>();

		global::Inventory oldInv = global::Inventory.instance;
		if ((UnityEngine.Object)(object)oldInv != (UnityEngine.Object)null)
		{
			global::Item[] array = UnityEngine.Object.FindObjectsOfType<global::Item>();
			foreach (global::Item val in array)
			{
				if ((UnityEngine.Object)(object)val == (UnityEngine.Object)null || !((Component)val).gameObject.activeInHierarchy)
				{
					continue;
				}
				bool placed;
				if (oldInv.HaveItem(val.ID))
				{
					placed = false;
				}
				else
				{
					placed = val.TakeItem();
				}
				if (!placed)
				{
					leftovers.Add(((Component)val).transform);
				}
			}
		}

		LinkedSquad.InventorySystem.Inventory newInv = LinkedSquad.InventorySystem.Inventory.Instance;
		if ((UnityEngine.Object)(object)newInv != (UnityEngine.Object)null && newInv.Items != null)
		{
			bool hasFree = newInv.Items.Any((LinkedSquad.InventorySystem.Item x) => (UnityEngine.Object)(object)x == (UnityEngine.Object)null);
			LinkedSquad.InventorySystem.Item[] array2 = UnityEngine.Object.FindObjectsOfType<LinkedSquad.InventorySystem.Item>();
			foreach (LinkedSquad.InventorySystem.Item val2 in array2)
			{
				if ((UnityEngine.Object)(object)val2 == (UnityEngine.Object)null || !((Component)val2).gameObject.activeInHierarchy)
				{
					continue;
				}
				if (hasFree)
				{
					try
					{
						newInv.AddItem(val2);
					}
					catch
					{
						leftovers.Add(((Component)val2).transform);
					}
				}
				else
				{
					leftovers.Add(((Component)val2).transform);
				}
			}
		}

		if (leftovers.Count == 0)
		{
			return;
		}
		FP_Controller fp = FP_Controller.instance;
		if ((UnityEngine.Object)(object)fp == (UnityEngine.Object)null)
		{
			return;
		}
		Transform camT = ((Camera.main != null) ? ((Component)Camera.main).transform : ((Component)fp).transform);
		Vector3 fwd = camT.forward;
		fwd.y = 0f;
		if (fwd.sqrMagnitude < 0.01f)
		{
			fwd = Vector3.forward;
		}
		fwd.Normalize();
		Vector3 drop = ((Component)fp).transform.position + fwd * 1.1f + Vector3.up * 0.5f;
		foreach (Transform tr in leftovers)
		{
			if (!((UnityEngine.Object)(object)tr == (UnityEngine.Object)null) && !((tr.position - drop).sqrMagnitude < 1.4f))
			{
				tr.position = drop + new Vector3(UnityEngine.Random.Range(-0.35f, 0.35f), UnityEngine.Random.Range(0f, 0.4f), UnityEngine.Random.Range(-0.35f, 0.35f));
			}
		}
	}

	private static void TickGod()
	{
		if (cheat_god)
		{
			ManiacLogic instance = ManiacLogic.instance;
			if ((UnityEngine.Object)(object)instance != (UnityEngine.Object)null && instance.IsChasing)
			{
				Traverse.Create((object)instance).Property("IsChasing", (object[])null).SetValue((object)false);
			}
			RestorePlayerControl();
		}
	}

	private static void RestorePlayerControl()
	{
		PlayerHead instance = PlayerHead.instance;
		if (!((UnityEngine.Object)(object)instance == (UnityEngine.Object)null))
		{
			if (instance.IsDisabledPlayer)
			{
				instance.DisablePlayer(false);
			}
			if (!instance.CanUseInteractive)
			{
				instance.SetCanUseInteractive(true);
			}
		}
	}

	private void HandleKeys()
	{
		if (Pressed(CheatAction.Menu) || (GetBind(CheatAction.Menu) == KeyCode.None && Input.GetKeyDown(KeyCode.F1)))
		{
			_menuOpen = !_menuOpen;
		}
		if (Pressed(CheatAction.God))
		{
			cheat_god = !cheat_god;
			CheatUi.self?.ToggleNote("Бессмертие", cheat_god);
			SavePrefs();
		}
		if (Pressed(CheatAction.Silent))
		{
			cheat_silent = !cheat_silent;
			CheatUi.self?.ToggleNote("Тишина", cheat_silent);
			SavePrefs();
		}
		if (Pressed(CheatAction.Fly))
		{
			ToggleFly();
		}
		if (Pressed(CheatAction.Freeze))
		{
			cheat_freeze = !cheat_freeze;
			CheatUi.self?.ToggleNote("Заморозка маньяка", cheat_freeze);
			SavePrefs();
		}
		if (Pressed(CheatAction.KeepItems))
		{
			cheat_keepitems = !cheat_keepitems;
			CheatUi.self?.ToggleNote("Сохранение предметов", cheat_keepitems);
			SavePrefs();
		}
		if (Pressed(CheatAction.EspManiac))
		{
			cheat_esp_maniac = !cheat_esp_maniac;
			SavePrefs();
		}
		if (Pressed(CheatAction.EspItems))
		{
			cheat_esp_items = !cheat_esp_items;
			SavePrefs();
		}
		if (Pressed(CheatAction.Bright))
		{
			cheat_fullbright = !cheat_fullbright;
			CheatUi.self?.ToggleNote("Полная яркость", cheat_fullbright);
			SavePrefs();
		}
		if (Pressed(CheatAction.SpeedDown))
		{
			SetSpeed(Mathf.Max(0.25f, (float)Math.Round(cheat_speedmult - 0.5f, 2)));
		}
		if (Pressed(CheatAction.SpeedUp))
		{
			SetSpeed(Mathf.Min(50f, (float)Math.Round(cheat_speedmult + 0.5f, 2)));
		}
		if (Pressed(CheatAction.UnlockAll))
		{
			UnlockAll();
		}
		if (Pressed(CheatAction.NotTrap))
		{
			SetNotTrap(!cheat_notrap);
			CheatUi.self?.ToggleNote("Анти-капкан", cheat_notrap);
		}
		if (Pressed(CheatAction.Steal))
		{
			SetSteal(!cheat_steal);
			CheatUi.self?.ToggleNote("Авосбор", cheat_steal);
		}
	}

	private static void LoadBinds()
	{
		foreach (CheatAction a in Enum.GetValues(typeof(CheatAction)))
		{
			_binds[a] = (KeyCode)PlayerPrefs.GetInt("metelcheat_bind_" + a, (int)KeyCode.None);
		}
	}

	public static KeyCode GetBind(CheatAction a) => _binds.TryGetValue(a, out var k) ? k : KeyCode.None;

	public static void SetBind(CheatAction a, KeyCode k)
	{
		if (k == KeyCode.None || k == KeyCode.Escape || k == KeyCode.Delete)
		{
			return;
		}
		_binds[a] = k;
		_bindCooldown[a] = Time.unscaledTime + 0.5f;
		PlayerPrefs.SetInt("metelcheat_bind_" + a, (int)k);
		PlayerPrefs.Save();
		CheatUi.self?.Message("Клавиша «" + CheatUi.ActionName(a) + "»: " + KeyLabel(k));
	}

	public static void ClearBind(CheatAction a)
	{
		_binds[a] = KeyCode.None;
		PlayerPrefs.SetInt("metelcheat_bind_" + a, (int)KeyCode.None);
		PlayerPrefs.Save();
		CheatUi.self?.Message("Клавиша «" + CheatUi.ActionName(a) + "» снята");
	}

	public static string KeyLabel(KeyCode k)
	{
		if (k == KeyCode.None)
		{
			return "n/a";
		}
		if (k >= KeyCode.F1 && k <= KeyCode.F15)
		{
			return "F" + (int)(k - KeyCode.F1 + 1);
		}
		switch (k)
		{
		case KeyCode.Mouse0:
			return "ЛКМ";
		case KeyCode.Mouse1:
			return "ПКМ";
		case KeyCode.Mouse2:
			return "СКМ";
		case KeyCode.Space:
			return "Пробел";
		case KeyCode.LeftShift:
		case KeyCode.RightShift:
			return "Shift";
		case KeyCode.LeftControl:
		case KeyCode.RightControl:
			return "Ctrl";
		case KeyCode.LeftAlt:
		case KeyCode.RightAlt:
			return "Alt";
		case KeyCode.Tab:
			return "Tab";
		case KeyCode.Return:
		case KeyCode.KeypadEnter:
			return "Enter";
		case KeyCode.Backspace:
			return "Backspace";
		case KeyCode.CapsLock:
			return "CapsLock";
		default:
			return k.ToString();
		}
	}

	private static readonly Dictionary<CheatAction, float> _bindCooldown = new Dictionary<CheatAction, float>();


	private static bool Pressed(CheatAction a)
	{
		if (IsMenuOpen)
		{
			return false;
		}
		if (_bindCooldown.TryGetValue(a, out var until) && Time.unscaledTime < until)
		{
			return false;
		}
		return _binds.TryGetValue(a, out var k) && k != KeyCode.None && Input.GetKeyDown(k);
	}

	public static void ToggleFly()
	{
		cheat_fly = !cheat_fly;
		FP_Controller instance = FP_Controller.instance;
		if ((UnityEngine.Object)(object)instance != (UnityEngine.Object)null)
		{
			if (cheat_fly)
			{
				_prevTrapped = instance.trapped;
				ApplyFlySideEffects(instance, on: true);
			}
			else
			{
				ApplyFlySideEffects(instance, on: false);
			}
		}
		CheatUi.self?.ToggleNote("Полёт", cheat_fly);
	}

	public static void ApplyFlySideEffects(FP_Controller fp, bool on)
	{
		CharacterController controller = fp.controller;
		if ((UnityEngine.Object)(object)controller != (UnityEngine.Object)null)
		{
			((Collider)controller).enabled = !on;
		}
		if (on)
		{
			fp.trapped = true;
		}
		else
		{
			fp.trapped = _prevTrapped;
		}
	}

	public static void SavePrefsStatic()
	{
		PlayerPrefs.SetInt("metelcheat_god", cheat_god ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_silent", cheat_silent ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_freeze", cheat_freeze ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_keepitems", cheat_keepitems ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_espm", cheat_esp_maniac ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_espi", cheat_esp_items ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_bright", cheat_fullbright ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_notrap", cheat_notrap ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_steal", cheat_steal ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void UnlockAllLevels() => UnlockAll();

	private void SavePrefs()
	{
		PlayerPrefs.SetInt("metelcheat_god", cheat_god ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_silent", cheat_silent ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_freeze", cheat_freeze ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_keepitems", cheat_keepitems ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_espm", cheat_esp_maniac ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_espi", cheat_esp_items ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_bright", cheat_fullbright ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_notrap", cheat_notrap ? 1 : 0);
		PlayerPrefs.SetInt("metelcheat_steal", cheat_steal ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void SetNotTrap(bool v)
	{
		cheat_notrap = v;
		PlayerPrefs.SetInt("metelcheat_notrap", v ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void SetSteal(bool v)
	{
		cheat_steal = v;
		PlayerPrefs.SetInt("metelcheat_steal", v ? 1 : 0);
		PlayerPrefs.Save();
		if (v)
		{
			_nextSteal = 0f;
		}
	}

	public static void SetSpeed(float m)
	{
		cheat_speedmult = Mathf.Clamp(m, 0.25f, 50f);
		PlayerPrefs.SetFloat("metelcheat_speed", cheat_speedmult);
		PlayerPrefs.Save();
		CheatUi.self?.Message("Скорость: " + SpeedLabel);
	}

	private static void UnlockAll()
	{
		PlayerPrefs.SetInt("WinLevelA", 1);
		PlayerPrefs.SetInt("WinLevelB", 1);
		PlayerPrefs.SetInt("WinLevelC", 1);
		PlayerPrefs.SetInt("WinLevelD", 1);
		PlayerPrefs.SetInt("EndGame", 1);
		PlayerPrefs.SetInt("MaxAlive", 1);
		PlayerPrefs.SetInt("PsychopathMask", 1);
		PlayerPrefs.SetInt("NoAds", 1);
		PlayerPrefs.Save();
		CheatUi.self?.Message("Уровни разблокированы");
	}

	private static void TickSilent()
	{
		if (cheat_silent)
		{
			Noise instance = Noise.instance;
			if ((UnityEngine.Object)(object)instance != (UnityEngine.Object)null)
			{
				instance.CanNoise = true;
				Traverse.Create((object)instance).Property("makedNoise", (object[])null).SetValue((object)false);
				Traverse.Create((object)instance).Field("Safe").SetValue((object)true);
			}
			ManiacLogic instance2 = ManiacLogic.instance;
			if ((UnityEngine.Object)(object)instance2 != (UnityEngine.Object)null && instance2.IsChasing)
			{
				Traverse.Create((object)instance2).Property("IsChasing", (object[])null).SetValue((object)false);
			}
			RestorePlayerControl();
		}
	}

	private static void TickSpeed()
	{
		FP_Controller instance = FP_Controller.instance;
		if (!((UnityEngine.Object)(object)instance == (UnityEngine.Object)null))
		{
			if ((UnityEngine.Object)(object)_speedOwner != (UnityEngine.Object)(object)instance)
			{
				_speedOwner = instance;
				_baseWalk = instance.walkSpeed;
				_baseRun = instance.runSpeed;
				_baseCrouch = instance.crouchSpeed;
			}
			instance.walkSpeed = _baseWalk * cheat_speedmult;
			instance.runSpeed = _baseRun * cheat_speedmult;
			instance.crouchSpeed = _baseCrouch * cheat_speedmult;
		}
	}

	private static void TickFly()
	{

		if (!cheat_fly)
		{
			return;
		}
		FP_Controller instance = FP_Controller.instance;
		if ((UnityEngine.Object)(object)instance == (UnityEngine.Object)null)
		{
			return;
		}
		CharacterController controller = instance.controller;
		if ((UnityEngine.Object)(object)controller != (UnityEngine.Object)null && ((Collider)controller).enabled)
		{
			((Collider)controller).enabled = false;
		}
		if (!instance.trapped)
		{
			instance.trapped = true;
		}
		Camera main = Camera.main;
		if (!((UnityEngine.Object)(object)main == (UnityEngine.Object)null))
		{
			float num = (Input.GetKey((KeyCode)119) ? 1f : 0f) - (Input.GetKey((KeyCode)115) ? 1f : 0f);
			float num2 = (Input.GetKey((KeyCode)100) ? 1f : 0f) - (Input.GetKey((KeyCode)97) ? 1f : 0f);
			float num3 = (Input.GetKey((KeyCode)101) ? 1f : 0f) - (Input.GetKey((KeyCode)113) ? 1f : 0f);
			if (num != 0f || num2 != 0f || num3 != 0f)
			{
				Vector3 val = ((Component)main).transform.forward * num + ((Component)main).transform.right * num2 + Vector3.up * num3;
				float num4 = 12f * cheat_speedmult * (Input.GetKey((KeyCode)304) ? 3f : 1f);
				Transform transform = ((Component)instance).transform;
				transform.position += val.normalized * num4 * Time.deltaTime;
			}
		}
	}

	private static void TickFreeze()
	{
		bool flag = cheat_freeze && ((UnityEngine.Object)(object)PlayerManager.instance == (UnityEngine.Object)null || !PlayerManager.instance.IsDead);
		if (flag == _wasFrozen)
		{
			return;
		}
		_wasFrozen = flag;
		if (!_frozenCacheBuilt)
		{
			frozen.Clear();
			NavMeshAgent[] array = UnityEngine.Object.FindObjectsOfType<NavMeshAgent>();
			foreach (NavMeshAgent val in array)
			{
				frozen.Add(new FrozenThing
				{
					Agent = val,
					BaseSpeed = val.speed,
					Anim = ((Component)val).GetComponentInChildren<Animator>()
				});
			}
			frozenMovers.Clear();
			Type[] array2 = new Type[4]
			{
				typeof(Dog),
				typeof(AIFrank),
				typeof(AIAdele),
				typeof(BehaviourAgent)
			};
			foreach (Type type in array2)
			{
				frozenMovers.AddRange(UnityEngine.Object.FindObjectsOfType(type).OfType<MonoBehaviour>());
			}
			_frozenCacheBuilt = true;
		}
		foreach (FrozenThing item in frozen)
		{
			if (!((UnityEngine.Object)(object)item.Agent == (UnityEngine.Object)null))
			{
				item.Agent.isStopped = flag;
				item.Agent.speed = (flag ? 0f : item.BaseSpeed);
				if ((UnityEngine.Object)(object)item.Anim != (UnityEngine.Object)null)
				{
					item.Anim.speed = (flag ? 0f : 1f);
				}
			}
		}
		foreach (MonoBehaviour frozenMover in frozenMovers)
		{
			if ((UnityEngine.Object)(object)frozenMover != (UnityEngine.Object)null)
			{
				((Behaviour)frozenMover).enabled = !flag;
			}
		}
		if (!flag)
		{
			_frozenCacheBuilt = false;
		}
	}

	public static void SetMenuOpen(bool v)
	{
		_menuOpen = v;
	}
}
