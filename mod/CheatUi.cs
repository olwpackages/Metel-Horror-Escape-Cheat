using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetelCheat;

public class CheatUi : MonoBehaviour
{
    public static CheatUi self;

    private static readonly Color ColBg = Hex(0x0D1015);
    private static readonly Color ColSide = Hex(0x10141B);
    private static readonly Color ColCard = Hex(0x151A23);
    private static readonly Color ColSel = Hex(0x1B2534);
    private static readonly Color ColAccent = Hex(0x3D9BFF);
    private static readonly Color ColAccentDim = Hex(0x2B5B8F);
    private static readonly Color ColText = Hex(0xE8EBF0);
    private static readonly Color ColDim = Hex(0x9AA3B2);
    private static readonly Color ColOff = Hex(0x2A303B);
    private static readonly Color ColTrack = Hex(0x242A36);

    private GUIStyle _stTitle, _stSection, _stTab, _stTabSel,
                     _stRow, _stRowDim, _stTransparent, _stEsp, _stEspShadow, _stNote, _stStatus, _stCardBtn, _stSmall, _stCheck, _stChip, _stKey, _stFps, _stIntro;
    private Texture2D _tWhite;
    private bool _stylesInit;

    private Vector2 _winPos = new Vector2(140f, 70f);
    private const float WinW = 1000f, WinH = 500f, SideW = 200f, HeaderH = 40f;
    private bool _dragging;
    private Vector2 _dragOffset;
    private int _tab;
    private static readonly string[] Tabs = { "Выживание", "ESP", "Мир", "Разное" };

    private int _activeSlider = -1;

    private Main.CheatAction? _capturing;
    private static readonly Main.CheatAction[] BindActions =
    {
        Main.CheatAction.Menu, Main.CheatAction.God, Main.CheatAction.Silent, Main.CheatAction.Fly,
        Main.CheatAction.Freeze, Main.CheatAction.KeepItems, Main.CheatAction.EspManiac, Main.CheatAction.EspItems,
        Main.CheatAction.Bright, Main.CheatAction.SpeedDown, Main.CheatAction.SpeedUp, Main.CheatAction.NotTrap,
        Main.CheatAction.Steal, Main.CheatAction.UnlockAll
    };
    private static readonly string[] BindNames =
    {
        "меню", "бессмертие", "тишина", "полёт",
        "заморозка", "сохранение предметов", "ESP маньяка", "ESP предметов",
        "яркость", "скорость −", "скорость +", "анти-капкан",
        "авосбор", "все уровни"
    };

    public static string ActionName(Main.CheatAction a)
    {
        for (int i = 0; i < BindActions.Length; i++)
            if (BindActions[i] == a) return BindNames[i];
        return a.ToString();
    }

    private static readonly Dictionary<string, Texture2D> _roundCache = new Dictionary<string, Texture2D>();

    private static Texture2D Rounded(float wf, float hf, float rf, Color c, bool tl = true, bool tr = true, bool bl = true, bool br = true)
    {
        int w = (int)wf, h = (int)hf, r = (int)Mathf.Clamp(rf, 2f, 100f);
        var key = w + ":" + h + ":" + r + ":" + ColorUtility.ToHtmlStringRGBA(c) + (tl ? "1" : "0") + (tr ? "1" : "0") + (bl ? "1" : "0") + (br ? "1" : "0");
        if (_roundCache.TryGetValue(key, out var tex) && tex != null) return tex;
        tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.hideFlags = (HideFlags)61;
        var fill = new Color32((byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f), 255);
        tex.SetPixels32(FillRounded(w, h, r, fill, tl, tr, bl, br));
        tex.Apply(false, false);
        _roundCache[key] = tex;
        return tex;
    }

    private static Texture2D RoundedGrad(float wf, float hf, float rf, Color top, Color bot, bool tl = true, bool tr = true, bool bl = true, bool br = true)
    {
        int w = (int)wf, h = (int)hf, r = (int)Mathf.Clamp(rf, 2f, 100f);
        var key = "G" + w + ":" + h + ":" + r + ":" + ColorUtility.ToHtmlStringRGBA(top) + ":" + ColorUtility.ToHtmlStringRGBA(bot) + (tl ? "1" : "0") + (tr ? "1" : "0") + (bl ? "1" : "0") + (br ? "1" : "0");
        if (_roundCache.TryGetValue(key, out var tex) && tex != null) return tex;
        tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.hideFlags = (HideFlags)61;
        var ct = new Color32((byte)(top.r * 255f), (byte)(top.g * 255f), (byte)(top.b * 255f), 255);
        var cb = new Color32((byte)(bot.r * 255f), (byte)(bot.g * 255f), (byte)(bot.b * 255f), 255);
        tex.SetPixels32(FillRoundedGrad(w, h, r, ct, cb, tl, tr, bl, br));
        tex.Apply(false, false);
        _roundCache[key] = tex;
        return tex;
    }

    private static Color32[] FillRounded(int w, int h, int r, Color32 fill, bool tl, bool tr, bool bl, bool br)
    {
        var px = new Color32[w * h];
        var empty = new Color32(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float mask = 1f;
                if (tl && x < r && y < r) mask = CornerAlpha(x, y, r);
                else if (tr && x >= w - r && y < r) mask = CornerAlpha(w - 1 - x, y, r);
                else if (bl && x < r && y >= h - r) mask = CornerAlpha(x, h - 1 - y, r);
                else if (br && x >= w - r && y >= h - r) mask = CornerAlpha(w - 1 - x, h - 1 - y, r);
                px[y * w + x] = mask >= 1f ? fill : (mask <= 0f ? empty : new Color32(fill.r, fill.g, fill.b, (byte)(mask * 255f)));
            }
        }
        return px;
    }

    private static Color32[] FillRoundedGrad(int w, int h, int r, Color32 top, Color32 bot, bool tl, bool tr, bool bl, bool br)
    {
        var px = new Color32[w * h];
        var empty = new Color32(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
        {
            float ty = h > 1 ? (float)y / (h - 1) : 0f;
            byte cr = (byte)(top.r + (bot.r - top.r) * ty);
            byte cg = (byte)(top.g + (bot.g - top.g) * ty);
            byte cb2 = (byte)(top.b + (bot.b - top.b) * ty);
            for (int x = 0; x < w; x++)
            {
                float mask = 1f;
                if (tl && x < r && y < r) mask = CornerAlpha(x, y, r);
                else if (tr && x >= w - r && y < r) mask = CornerAlpha(w - 1 - x, y, r);
                else if (bl && x < r && y >= h - r) mask = CornerAlpha(x, h - 1 - y, r);
                else if (br && x >= w - r && y >= h - r) mask = CornerAlpha(w - 1 - x, h - 1 - y, r);
                px[y * w + x] = mask >= 1f ? new Color32(cr, cg, cb2, 255) : (mask <= 0f ? empty : new Color32(cr, cg, cb2, (byte)(mask * 255f)));
            }
        }
        return px;
    }

    private static float CornerAlpha(int cx, int cy, int r)
    {
        float dx = r - cx - 0.5f, dy = r - cy - 0.5f;
        float d = Mathf.Sqrt(dx * dx + dy * dy);
        return Mathf.Clamp01(r - d + 0.5f);
    }

    private readonly List<UnityEngine.UI.Image> _dimmed = new List<UnityEngine.UI.Image>();
    private float _nextOverlayScan;

    private int _fpsFrames;
    private float _fpsTimer;
    private int _fps;

    private bool _introStarted;
    private float _introT0;
    private bool _introFinished;

    private readonly List<EspTarget> _esp = new List<EspTarget>();
    private float _nextScan;

    private struct EspTarget
    {
        public Transform Tr;
        public Renderer Rend;
        public string Label;
        public bool Maniac;
    }

    private static readonly Dictionary<int, string> _oldNames = new Dictionary<int, string>();
    private static readonly Dictionary<string, string> _newNames = new Dictionary<string, string>();

    private static bool IsDummyName(string s)
    {
        if (string.IsNullOrEmpty(s)) return true;
        s = s.Trim();
        if (s.Length == 0 || s == "Unknown") return true;
        return System.Text.RegularExpressions.Regex.IsMatch(s, @"^[iI]tem[\s_]*\d+$");
    }

    private static string OldName(global::Item it)
    {
        int id = it.ID;
        if (!IsDummyName(it.ItemName)) return it.ItemName;
        if (_oldNames.TryGetValue(id, out var cached)) return cached;
        string resolved = null;
        var inv = global::Inventory.instance;
        if (inv != null)
        {
            var db = inv.GetDBItemByID(id);
            if (db != null && !IsDummyName(db.ItemName)) resolved = db.ItemName;
        }
        if (resolved == null)
        {
            try { var ndb = LinkedSquad.InventorySystem.ItemDisplayManager.GetItem(id.ToString()); if (ndb != null && !IsDummyName(ndb.itemName)) resolved = ndb.itemName; }
            catch { }
        }
        if (resolved == null) resolved = "Предмет " + id;
        _oldNames[id] = resolved;
        return resolved;
    }

    private static string NewName(LinkedSquad.InventorySystem.Item it)
    {
        if (!IsDummyName(it.itemName)) return it.itemName;
        string id = it.id;
        if (string.IsNullOrEmpty(id)) return "Предмет";
        if (_newNames.TryGetValue(id, out var cached)) return cached;
        string resolved = null;
        try { var db = LinkedSquad.InventorySystem.ItemDisplayManager.GetItem(id); if (db != null && !IsDummyName(db.itemName)) resolved = db.itemName; }
        catch { }
        if (resolved == null && int.TryParse(id, out var num))
        {
            try { var odb = global::Inventory.instance?.GetDBItemByID(num); if (odb != null && !IsDummyName(odb.ItemName)) resolved = odb.ItemName; }
            catch { }
        }
        if (resolved == null) resolved = Prettify(it.gameObject.name);
        if (IsDummyName(resolved)) resolved = "Предмет " + (int.TryParse(id, out var n2) ? n2.ToString() : "");
        _newNames[id] = resolved;
        return resolved;
    }

    private static string Prettify(string s)
    {
        if (string.IsNullOrEmpty(s)) return "предмет";
        s = s.Replace("(Clone)", "").Replace("_", " ").Trim();
        return s.Length == 0 ? "предмет" : s;
    }

    private void Update()
    {

        if (!_introStarted)
        {
            _introStarted = true;
            _introT0 = Time.unscaledTime;
        }

        _fpsFrames++;
        _fpsTimer += Time.unscaledDeltaTime;
        if (_fpsTimer >= 0.5f)
        {
            _fps = Mathf.RoundToInt(_fpsFrames / _fpsTimer);
            _fpsFrames = 0;
            _fpsTimer = 0f;
        }

        if (Time.unscaledTime < _nextScan) return;
        _nextScan = Time.unscaledTime + 1f;
        _esp.Clear();
        if (Main.cheat_esp_maniac)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag("Maniac"))
                _esp.Add(new EspTarget { Tr = go.transform, Rend = go.GetComponentInChildren<Renderer>(), Label = "маньяк", Maniac = true });
        }
        if (Main.cheat_esp_items)
        {
            foreach (var it in UnityEngine.Object.FindObjectsOfType<global::Item>())
                if (it.gameObject.activeInHierarchy)
                    _esp.Add(new EspTarget { Tr = it.transform, Rend = it.GetComponentInChildren<Renderer>(), Label = OldName(it), Maniac = false });
            foreach (var it in UnityEngine.Object.FindObjectsOfType<LinkedSquad.InventorySystem.Item>())
                if (it.gameObject.activeInHierarchy)
                    _esp.Add(new EspTarget { Tr = it.transform, Rend = it.GetComponentInChildren<Renderer>(), Label = NewName(it), Maniac = false });
        }
    }

    private void Awake()
    {
        self = this;
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }

    public void Message(string text)
    {

    }

    public void ToggleNote(string label, bool state)
    {

    }

    public void ResetStyles() => _stylesInit = false;

    private static Color Hex(int v)
    {
        return new Color32((byte)((v >> 16) & 255), (byte)((v >> 8) & 255), (byte)(v & 255), 255);
    }

    private void InitStyles()
    {
        _tWhite = Texture2D.whiteTexture;
        _stTitle = Label(Color.white, 16, FontStyle.Bold, TextAnchor.MiddleLeft, true);
        _stSection = Label(ColDim, 12, FontStyle.Normal, TextAnchor.MiddleLeft);
        _stTab = Label(ColText, 14, FontStyle.Normal, TextAnchor.MiddleLeft);
        _stTabSel = Label(ColAccent, 14, FontStyle.Normal, TextAnchor.MiddleLeft);
        _stRow = Label(ColText, 13, FontStyle.Normal, TextAnchor.MiddleLeft);
        _stRowDim = Label(ColDim, 11, FontStyle.Normal, TextAnchor.MiddleRight);
        _stChip = Label(ColDim, 11, FontStyle.Normal, TextAnchor.MiddleCenter, true);
        _stKey = Label(ColDim, 11, FontStyle.Normal, TextAnchor.MiddleRight, true);
        _stFps = Label(Color.white, 14, FontStyle.Normal, TextAnchor.MiddleCenter);
        _stIntro = Label(Color.white, 22, FontStyle.Bold, TextAnchor.MiddleCenter, true);
        _stEsp = Label(Color.white, 12, FontStyle.Normal, TextAnchor.MiddleCenter, true);
        _stEspShadow = new GUIStyle(_stEsp);
        _stEspShadow.normal.textColor = new Color(0f, 0f, 0f, 0.9f);
        _stNote = Label(new Color32(0x9A, 0xD5, 0x9A, 255), 13, FontStyle.Normal, TextAnchor.MiddleLeft, true);
        _stStatus = Label(Color.white, 12, FontStyle.Normal, TextAnchor.MiddleLeft, true);
        _stSmall = Label(ColDim, 11, FontStyle.Normal, TextAnchor.MiddleLeft, true);
        _stCheck = Label(Color.white, 11, FontStyle.Normal, TextAnchor.MiddleCenter);
        _stTransparent = new GUIStyle(GUI.skin.button);
        _stTransparent.normal.background = null;
        _stTransparent.hover.background = null;
        _stTransparent.active.background = null;
        _stTransparent.focused.background = null;
        _stCardBtn = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = ColText, background = Rounded(344f, 32f, 8f, ColSel) },
            hover = { textColor = ColAccent, background = Rounded(344f, 32f, 8f, Hex(0x223447)) },
            active = { textColor = ColAccent }
        };
        _stylesInit = true;
    }

    private static GUIStyle Label(Color c, int size, FontStyle style, TextAnchor anchor, bool rich = false)
    {
        return new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = style, alignment = anchor, richText = rich, wordWrap = false, normal = { textColor = c } };
    }

    private void OnGUI()
    {

        GUI.color = Color.white;
        GUI.contentColor = Color.white;
        GUI.backgroundColor = Color.white;
        GUI.depth = -999;

        if (!_stylesInit || _stCardBtn == null || _stCardBtn.normal.background == null)
            InitStyles();

        var cam = Camera.main;

        if (cam != null && _esp.Count > 0)
        {
            foreach (var t in _esp)
            {
                if (t.Tr == null) continue;
                if (!t.Tr.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(cam.transform.position, t.Tr.position);

                if (t.Rend != null)
                {

                    var b = t.Rend.bounds;
                    float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
                    for (int ci = 0; ci < 8; ci++)
                    {
                        var c = b.center + new Vector3(
                            (ci & 1) == 0 ? -b.extents.x : b.extents.x,
                            (ci & 2) == 0 ? -b.extents.y : b.extents.y,
                            (ci & 4) == 0 ? -b.extents.z : b.extents.z);
                        var sp = cam.WorldToScreenPoint(c);
                        if (sp.z <= 0f) continue;
                        float sx = sp.x, yy = Screen.height - sp.y;
                        if (sx < minx) minx = sx;
                        if (sx > maxx) maxx = sx;
                        if (yy < miny) miny = yy;
                        if (yy > maxy) maxy = yy;
                    }
                    if (minx > maxx || miny > maxy)
                    {
                        if (!t.Maniac) continue;

                        DrawTag(Screen.width - cam.WorldToScreenPoint(t.Tr.position).x, Screen.height - 40f, t.Label, d, t.Maniac);
                        continue;
                    }
                    minx -= 4f; miny -= 4f; maxx += 4f; maxy += 4f;

                    Color col = t.Maniac
                        ? (d < 5f ? Hex(0xE64545) : (d < 12f ? Hex(0xE6A23C) : Hex(0x67C23A)))
                        : Hex(0x53B7E8);

                    var prevC = GUI.color;
                    GUI.color = new Color(col.r, col.g, col.b, 0.10f * GUI.color.a);
                    GUI.DrawTexture(new Rect(minx, miny, maxx - minx, maxy - miny), _tWhite, ScaleMode.StretchToFill);
                    GUI.color = prevC;
                    DrawBracket(minx, miny, maxx - minx, maxy - miny, t.Maniac ? Hex(0xFF5555) : Hex(0x66CCFF));

                    string color = "#" + ColorUtility.ToHtmlStringRGB(col);
                    string text = "<color=" + color + ">" + t.Label + " · " + d.ToString("0") + " м</color>";
                    var r = new Rect((minx + maxx) / 2f - 110f, miny - 22f, 220f, 20f);
                    GUI.Label(r, text, _stEspShadow);
                    GUI.Label(new Rect(r.x - 1f, r.y - 1f, r.width, r.height), text, _stEsp);
                }
                else
                {

                    var sp = cam.WorldToScreenPoint(t.Tr.position);
                    bool behind = sp.z <= 0f;
                    float x = sp.x, y2 = Screen.height - sp.y;
                    if (t.Maniac && behind) { x = Screen.width - x; y2 = Screen.height - 40f; }
                    if (!t.Maniac && behind) continue;
                    if (x < -100f || x > Screen.width + 100f || y2 < -40f || y2 > Screen.height + 40f) continue;
                    DrawTag(x, y2, t.Label, d, t.Maniac);
                }
            }
        }

        DrawActiveList();

        {
            const float mh = 30f, fh = 26f, gap = 5f;
            const float mw = 300f, fw = 140f;
            var mbar = new Rect((Screen.width - mw) / 2f, 12f, mw, mh);
            GUI.DrawTexture(mbar, Rounded(mw, mh, 6f, Hex(0x1B2534)), ScaleMode.StretchToFill);
            GUI.Label(new Rect(mbar.x, 11f, mw, mh), "Metel made by t.me/s1lentpacket", _stFps);
            var fbar = new Rect((Screen.width - fw) / 2f, 12f + mh + gap, fw, fh);
            GUI.DrawTexture(fbar, Rounded(fw, fh, 6f, Hex(0x1B2534)), ScaleMode.StretchToFill);
            GUI.Label(new Rect(fbar.x, 11f + mh + gap, fw, fh), "Fps - " + _fps, _stFps);
        }

        if (_introStarted && !_introFinished) DrawIntro();

        if (!Main.IsMenuOpen)
        {
            if (_dimmed.Count > 0) RestoreOverlays();
            return;
        }

        _winPos.x = Mathf.Clamp(_winPos.x, 8f, Mathf.Max(8f, Screen.width - WinW - 8f));
        _winPos.y = Mathf.Clamp(_winPos.y, 8f, Mathf.Max(8f, Screen.height - WinH - 8f));

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ScanOverlays();

        var e = Event.current;

        if (_capturing != null)
        {
            var act = _capturing.Value;
            if (e.type == EventType.KeyDown && e.keyCode != KeyCode.None)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    _capturing = null;
                    Message("Бинд отменён");
                }
                else if (e.keyCode == KeyCode.Delete)
                {
                    Main.ClearBind(act);
                    _capturing = null;
                }
                else
                {
                    Main.SetBind(act, e.keyCode);
                    _capturing = null;
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDown)
            {

                _capturing = null;
                Message("Бинд отменён");
            }
        }

        var headerRect = new Rect(_winPos.x, _winPos.y, WinW, HeaderH);
        if (e.type == EventType.MouseDown && headerRect.Contains(e.mousePosition))
        {
            _dragging = true;
            _dragOffset = e.mousePosition - _winPos;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && _dragging)
        {
            _winPos = e.mousePosition - _dragOffset;
            e.Use();
        }
        else if (e.type == EventType.MouseUp && _dragging)
        {
            _dragging = false;
            e.Use();
        }

        GUI.DrawTexture(new Rect(_winPos.x, _winPos.y, WinW, WinH), Rounded(WinW, WinH, 18f, ColBg), ScaleMode.StretchToFill);
        var side = new Rect(_winPos.x, _winPos.y + HeaderH, SideW, WinH - HeaderH);
        GUI.DrawTexture(side, Rounded(SideW, WinH - HeaderH, 18f, ColSide, true, false, false, true), ScaleMode.StretchToFill);
        GUI.Label(new Rect(_winPos.x + 18f, _winPos.y + 7f, SideW - 20f, 26f), "<color=#3D9BFF>Metel Cheat</color>", _stTitle);

        GUI.Label(new Rect(side.x + 18f, side.y + 14f, SideW - 30f, 20f), "Меню", _stSection);
        float sy = side.y + 42f;
        for (int i = 0; i < Tabs.Length; i++)
        {
            var tr = new Rect(side.x + 8f, sy, SideW - 16f, 34f);
            if (i == _tab) GUI.DrawTexture(tr, Rounded(tr.width, tr.height, 8f, ColSel), ScaleMode.StretchToFill);
            if (GUI.Button(tr, "", _stTransparent)) _tab = i;
            GUI.Label(new Rect(side.x + 22f, sy, SideW - 40f, 34f), Tabs[i], i == _tab ? _stTabSel : _stTab);
            if (i == _tab)
            {
                var pc = GUI.color;
                GUI.color = ColAccent;
                GUI.DrawTexture(new Rect(side.x, sy + 6f, 3f, 22f), _tWhite, ScaleMode.StretchToFill);
                GUI.color = pc;
            }
            sy += 38f;
        }

        float lx = _winPos.x + SideW + 20f;
        float ty = _winPos.y + HeaderH + 18f;
        float cw = 372f;
        float cx2 = lx + cw + 18f;
        float gy = 14f;

        if (_tab == 0)
        {

            float y = ty;
            Card(lx, y, cw, 134f, "Защита");
            y += 36f;
            y += CheckRow(lx, y, cw, Main.cheat_god, "Бессмертие", Main.CheatAction.God, v => { Main.cheat_god = v; Main.SavePrefsStatic(); });
            y += CheckRow(lx, y, cw, Main.cheat_silent, "Тишина — маньяк не слышит", Main.CheatAction.Silent, v => { Main.cheat_silent = v; Main.SavePrefsStatic(); });
            y += CheckRow(lx, y, cw, Main.cheat_keepitems, "Не терять предметы", Main.CheatAction.KeepItems, v => { Main.cheat_keepitems = v; Main.SavePrefsStatic(); });

            y = ty + 134f + gy;
            Card(lx, y, cw, 132f, "Движение");
            y += 36f;
            y += CheckRow(lx, y, cw, Main.cheat_fly, "Полёт — WASD, E/Q, Shift", Main.CheatAction.Fly, v => { if (v != Main.cheat_fly) Main.ToggleFly(); });
            SliderRow(lx, y, cw, 0, 0.25f, 10f, "Скорость игрока", v => { Main.cheat_speedmult = v; Main.SavePrefsStatic(); });

            Card(cx2, ty, cw, 108f, "Освещение");
            float y3 = ty + 36f;
            y3 += CheckRow(cx2, y3, cw, Main.cheat_fullbright, "Полная яркость", Main.CheatAction.Bright, v => { Main.cheat_fullbright = v; Main.SavePrefsStatic(); });
            GUI.Label(new Rect(cx2 + 14f, y3 + 2f, cw - 28f, 18f), "тёмная сцена светится целиком, туман выключен", _stSmall);
        }
        else if (_tab == 1)
        {

            float y = ty;
            Card(lx, y, cw, 106f, "Сущности");
            y += 36f;
            y += CheckRow(lx, y, cw, Main.cheat_esp_maniac, "ESP маньяка", Main.CheatAction.EspManiac, v => { Main.cheat_esp_maniac = v; Main.SavePrefsStatic(); });
            y += CheckRow(lx, y, cw, Main.cheat_esp_items, "ESP предметов и ключей", Main.CheatAction.EspItems, v => { Main.cheat_esp_items = v; Main.SavePrefsStatic(); });

            Card(cx2, ty, cw, 158f, "Цвета меток");
            float y2 = ty + 40f;
            GUI.Label(new Rect(cx2 + 14f, y2, cw - 28f, 20f), "<color=#7fd57f>зелёный</color> — маньяк дальше 12 м", _stSmall);
            GUI.Label(new Rect(cx2 + 14f, y2 + 22f, cw - 28f, 20f), "<color=#ffaa44>оранжевый</color> — ближе 12 м", _stSmall);
            GUI.Label(new Rect(cx2 + 14f, y2 + 44f, cw - 28f, 20f), "<color=#ff5555>красный</color> — ближе 5 м, беги", _stSmall);
            GUI.Label(new Rect(cx2 + 14f, y2 + 66f, cw - 28f, 20f), "<color=#66ccff>голубой</color> — предметы и ключи", _stSmall);
            GUI.Label(new Rect(cx2 + 14f, y2 + 92f, cw - 28f, 20f), "цель за спиной — метка уходит к низу экрана", _stSmall);
        }
        else if (_tab == 2)
        {

            float y = ty;
            Card(lx, y, cw, 106f, "Маньяк");
            y += 36f;
            y += CheckRow(lx, y, cw, Main.cheat_freeze, "Заморозка маньяка", Main.CheatAction.Freeze, v => { Main.cheat_freeze = v; Main.SavePrefsStatic(); });
            y += CheckRow(lx, y, cw, Main.cheat_notrap, "Анти-капкан", Main.CheatAction.NotTrap, v => Main.SetNotTrap(v));

            Card(cx2, ty, cw, 100f, "Предметы");
            CheckRow(cx2, ty + 36f, cw, Main.cheat_steal, "Авосбор всех предметов", Main.CheatAction.Steal, v => Main.SetSteal(v));
            GUI.Label(new Rect(cx2 + 14f, ty + 66f, cw - 28f, 18f), "каждые 1.2 с подтягивает всё из мира в инвентарь", _stSmall);
        }
        else
        {

            float y = ty;
            Card(lx, y, cw, 136f, "Прогресс");
            if (GUI.Button(new Rect(lx + 14f, y + 38f, cw - 28f, 32f), "Открыть все уровни  [" + Main.KeyLabel(Main.GetBind(Main.CheatAction.UnlockAll)) + "]", _stCardBtn))
            {
                Main.UnlockAllLevels();
            }
            GUI.Label(new Rect(lx + 14f, y + 76f, cw - 28f, 20f), "клавишу ставишь кликом по чипу, Delete её снимает", _stSmall);
            string mlbl = Main.GetBind(Main.CheatAction.Menu) != KeyCode.None ? "Клавиша меню" : "Клавиша меню — сейчас запасной F1";
            CheckRow(lx, y + 100f, cw, false, mlbl, Main.CheatAction.Menu, v => { }, clickable: false);

            float keysCardH = 36f + BindActions.Length * 26f + 6f;
            Card(cx2, ty, cw, keysCardH, "Горячие клавиши");
            float ky = ty + 40f;
            for (int i = 0; i < BindActions.Length; i++)
            {
                var a = BindActions[i];
                KeyCode kl = Main.GetBind(a);
                var bar = new Rect(cx2 + 14f, ky, cw - 28f, 22f);
                GUI.DrawTexture(bar, Rounded(bar.width, 22f, 5f, Hex(0x1B2534)), ScaleMode.StretchToFill);
                GUI.Label(new Rect(bar.x + 12f, ky - 1f, bar.width - 100f, 22f), BindNames[i], _stRow);
                string shown = kl != KeyCode.None
                    ? "<color=#3D9BFF>" + Main.KeyLabel(kl) + "</color>"
                    : (a == Main.CheatAction.Menu ? "<color=#3D9BFF>n/a</color> · F1" : "<color=#3D9BFF>n/a</color>");
                GUI.Label(new Rect(bar.x + bar.width - 92f, ky - 1f, 80f, 22f), shown, _stKey);
                ky += 26f;
            }
        }
    }

    private static bool StateOf(Main.CheatAction a)
    {
        switch (a)
        {
        case Main.CheatAction.God: return Main.cheat_god;
        case Main.CheatAction.Silent: return Main.cheat_silent;
        case Main.CheatAction.Fly: return Main.cheat_fly;
        case Main.CheatAction.Freeze: return Main.cheat_freeze;
        case Main.CheatAction.KeepItems: return Main.cheat_keepitems;
        case Main.CheatAction.Bright: return Main.cheat_fullbright;
        case Main.CheatAction.EspManiac: return Main.cheat_esp_maniac;
        case Main.CheatAction.EspItems: return Main.cheat_esp_items;
        case Main.CheatAction.NotTrap: return Main.cheat_notrap;
        case Main.CheatAction.Steal: return Main.cheat_steal;
        default: return false;
        }
    }

    private void DrawActiveList()
    {
        const float w = 252f, bh = 24f, gap = 4f;
        var act = new List<Main.CheatAction>();
        foreach (var a in BindActions)
            if (StateOf(a) && Main.GetBind(a) != KeyCode.None)
                act.Add(a);
        if (act.Count == 0) return;

        float x = Screen.width - w - 14f;
        float y = 14f;
        foreach (var a in act)
        {
            var bar = new Rect(x, y, w, bh);
            GUI.DrawTexture(bar, Rounded(w, bh, 5f, Hex(0x1B2534)), ScaleMode.StretchToFill);
            GUI.Label(new Rect(bar.x + 12f, y - 1f, w - 84f, bh), ActionName(a), _stRow);
            KeyCode k = Main.GetBind(a);
            string kshown = k != KeyCode.None ? Main.KeyLabel(k) : "<color=#3D9BFF>n/a</color>";
            GUI.Label(new Rect(bar.x + w - 74f, y - 1f, 62f, bh), kshown, _stKey);
            y += bh + gap;
        }
    }

    private void DrawIntro()
    {
        float t = Time.unscaledTime - _introT0;
        const float fadeIn = 0.5f;
        const float textIn = 1.1f;
        const float textOut = 2.8f;
        const float textGone = 3.6f;
        const float blurOut = 4.4f;

        if (t >= blurOut)
        {
            _introFinished = true;
            Main.OpenMenu();
            return;
        }

        float veilA;
        if (t < fadeIn) veilA = t / fadeIn * 0.62f;
        else if (t < blurOut - 0.8f) veilA = 0.62f;
        else veilA = Mathf.Lerp(0.62f, 0f, (t - (blurOut - 0.8f)) / 0.8f);

        var prev = GUI.color;
        GUI.color = new Color(0.03f, 0.045f, 0.065f, veilA);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _tWhite, ScaleMode.StretchToFill);
        GUI.color = Color.white;

        float textA = t < textIn ? (t - fadeIn) / (textIn - fadeIn)
                    : t < textOut ? 1f
                    : Mathf.Clamp01(1f - (t - textOut) / (textGone - textOut));

        if (textA > 0.001f)
        {
            float bw = 520f, bh = 56f;
            var bar = new Rect((Screen.width - bw) / 2f, Screen.height / 2f - bh / 2f, bw, bh);
            GUI.color = new Color(1f, 1f, 1f, textA);
            GUI.DrawTexture(bar, Rounded(bw, bh, 8f, Hex(0x1B2534)), ScaleMode.StretchToFill);
            GUI.Label(new Rect(bar.x, bar.y - 1f, bw, bh), "Metel Cheat made by <color=#3D9BFF>t.me/s1lentpacket</color>", _stIntro);
        }
        GUI.color = prev;
    }

    private void ScanOverlays()
    {
        if (Time.unscaledTime < _nextOverlayScan) return;
        _nextOverlayScan = Time.unscaledTime + 1f;
        foreach (var img in UnityEngine.Object.FindObjectsOfType<UnityEngine.UI.Image>())
        {
            if (img == null || _dimmed.Contains(img)) continue;
            if (!img.isActiveAndEnabled) continue;

            float eff = img.color.a;
            var t0 = img.transform;
            while (t0 != null)
            {
                var cg = t0.GetComponent<CanvasGroup>();
                if (cg != null) eff *= cg.alpha;
                if (t0.parent == null) break;
                t0 = t0.parent;
            }
            if (eff <= 0.02f || eff >= 0.97f) continue;

            var c = img.color;
            if (Mathf.Max(c.r, Mathf.Max(c.g, c.b)) > 0.2f) continue;
            var rt = img.rectTransform;
            if (rt.rect.width < Screen.width * 0.7f || rt.rect.height < Screen.height * 0.7f) continue;
            img.enabled = false;
            _dimmed.Add(img);
            MelonLoader.MelonLogger.Msg("оверлей приглушен: " + img.gameObject.name + " alpha=" + eff.ToString("0.00") + " canvas=" + (img.canvas != null ? img.canvas.name : "?") + " order=" + (img.canvas != null ? img.canvas.sortingOrder : 0));
        }
    }

    private void RestoreOverlays()
    {
        foreach (var img in _dimmed)
            if (img != null) img.enabled = true;
        _dimmed.Clear();
        _nextOverlayScan = 0f;
    }

    private void DrawBracket(float x, float y, float w, float h, Color c)
    {
        float len = Mathf.Clamp(Mathf.Min(w, h) * 0.22f, 7f, 24f);
        float th = 2f;
        var prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(new Rect(x, y, len, th), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x, y, th, len), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x + w - len, y, len, th), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x + w - th, y, th, len), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x, y + h - th, len, th), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x, y + h - len, th, len), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x + w - len, y + h - th, len, th), _tWhite, ScaleMode.StretchToFill);
        GUI.DrawTexture(new Rect(x + w - th, y + h - len, th, len), _tWhite, ScaleMode.StretchToFill);
        GUI.color = prev;
    }

    private void DrawTag(float x, float y2, string label, float d, bool maniac)
    {
        string color = maniac ? (d < 5f ? "#ff5555" : (d < 12f ? "#ffaa44" : "#7fd57f")) : "#66ccff";
        string text = "<color=" + color + ">" + label + " · " + d.ToString("0") + " м</color>";
        var r = new Rect(x - 110f, y2 - 10f, 220f, 20f);
        GUI.Label(r, text, _stEspShadow);
        GUI.Label(new Rect(r.x - 1f, r.y - 1f, r.width, r.height), text, _stEsp);
    }

    private void Card(float x, float y, float w, float h, string caption)
    {
        GUI.DrawTexture(new Rect(x, y, w, h), Rounded(w, h, 10f, ColCard), ScaleMode.StretchToFill);
        GUI.Label(new Rect(x + 14f, y + 5f, w - 28f, 22f), caption, _stSection);
        var pc = GUI.color;
        GUI.color = ColAccent;
        GUI.DrawTexture(new Rect(x, y + 6f, 2.5f, 26f), _tWhite, ScaleMode.StretchToFill);
        GUI.color = pc;
    }

    private float CheckRow(float x, float y, float w, bool state, string label, Main.CheatAction bind, Action<bool> apply, bool clickable = true)
    {
        var r = new Rect(x + 12f, y, w - 24f, 26f);
        const float chipW = 74f;
        var mainHit = new Rect(r.x, r.y, r.width - chipW - 4f, r.height);
        bool clicked = GUI.Button(mainHit, "", _stTransparent);

        var e = Event.current;
        if (mainHit.Contains(e.mousePosition))
        {
            var hc = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.045f);
            GUI.DrawTexture(mainHit, _tWhite, ScaleMode.StretchToFill);
            GUI.color = hc;
        }

        var cb = new Rect(r.x, r.y + 5f, 16f, 16f);
        var prev = GUI.color;
        GUI.color = state ? ColAccent : ColOff;
        GUI.DrawTexture(cb, Rounded(16f, 16f, 4f, Color.white), ScaleMode.StretchToFill);
        GUI.color = prev;
        if (state) GUI.Label(new Rect(cb.x + 2f, cb.y - 2f, 16f, 20f), "✓", _stCheck);
        GUI.Label(new Rect(r.x + 24f, r.y - 1f, mainHit.width - 28f, 22f), label, _stRow);

        var chip = new Rect(r.xMax - chipW, r.y + 1f, chipW, 24f);
        bool capturing = _capturing == bind;
        GUI.DrawTexture(chip, Rounded(chipW, 24f, 6f, capturing ? ColAccentDim : Hex(0x1B2029)), ScaleMode.StretchToFill);
        KeyCode curKey = Main.GetBind(bind);
        string chipText = capturing ? "…" : (curKey != KeyCode.None ? Main.KeyLabel(curKey) : "<color=#3D9BFF>n/a</color>");
        GUI.Label(chip, chipText, _stChip);
        if (e.type == EventType.MouseUp && chip.Contains(e.mousePosition) && !capturing)
        {
            _capturing = bind;
            e.Use();
        }

        if (clicked && clickable)
        {
            apply(!state);
            Message(label + ": " + (!state ? "вкл" : "выкл"));
        }
        return 28f;
    }

    private void SliderRow(float x, float y, float w, int id, float min, float max, string label, Action<float> apply)
    {
        var e = Event.current;
        var track = new Rect(x + 12f, y + 24f, w - 24f, 18f);

        if (e.type == EventType.MouseDown && track.Contains(e.mousePosition))
        {
            _activeSlider = id;
            e.Use();
        }
        bool active = _activeSlider == id;

        if (active && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown))
        {
            float t = Mathf.Clamp01((e.mousePosition.x - track.x - 6f) / (track.width - 12f));
            float v = Mathf.Round(Mathf.Lerp(min, max, t) * 100f) / 100f;
            if (Mathf.Abs(v - Main.cheat_speedmult) > 0.001f) apply(v);
            e.Use();
        }
        if (active && e.type == EventType.MouseUp) _activeSlider = -1;

        GUI.Label(new Rect(x + 12f, y, w * 0.6f, 20f), label, _stRow);
        GUI.Label(new Rect(x + w - 150f, y, 138f, 20f), Main.cheat_speedmult.ToString("0.00") + "x", _stRowDim);

        float tt = Mathf.InverseLerp(min, max, Main.cheat_speedmult);
        float knobX = track.x + tt * (track.width - 12f);
        var pc = GUI.color;
        GUI.color = ColTrack;
        GUI.DrawTexture(new Rect(track.x, track.y + 7f, track.width, 4f), _tWhite, ScaleMode.StretchToFill);
        GUI.color = ColAccent;
        GUI.DrawTexture(new Rect(track.x, track.y + 7f, knobX - track.x + 6f, 4f), _tWhite, ScaleMode.StretchToFill);
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(knobX, track.y + 3f, 12f, 12f), Rounded(12f, 12f, 6f, Color.white), ScaleMode.StretchToFill);
        GUI.color = pc;
    }
}
