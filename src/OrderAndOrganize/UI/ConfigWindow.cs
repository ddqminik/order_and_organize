using OrderAndOrganize.Configuration;
using OrderAndOrganize.Services;
using UnityEngine;

namespace OrderAndOrganize.UI
{
    public class ConfigWindow
    {
        private readonly ModConfiguration _config;
        private readonly System.Func<bool> _getAutoEnabled;
        private readonly System.Action _toggleAuto;

        private bool _visible;
        private Rect _windowRect = new Rect(20f, 20f, 420f, 620f);
        private int _windowId;

        private int _thresholdInput;
        private float _cashReserveInput;
        private int _scanIntervalInput;
        private int _pendingTimeoutInput;
        private string _buttonTextInput;
        private Vector2 _scrollPos;

        private GUIStyle _headerStyle;
        private GUIStyle _sectionStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _valueStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _windowStyle;
        private bool _stylesInitialized;

        public bool IsVisible => _visible;

        public ConfigWindow(
            ModConfiguration config,
            System.Func<bool> getAutoEnabled,
            System.Action toggleAuto)
        {
            _config = config;
            _getAutoEnabled = getAutoEnabled;
            _toggleAuto = toggleAuto;
            _windowId = "OAOConfig".GetHashCode();
            SyncFromConfig();
        }

        public void Toggle()
        {
            _visible = !_visible;
            if (_visible)
                SyncFromConfig();
        }

        private void SyncFromConfig()
        {
            _thresholdInput = _config.ThresholdUnits.Value;
            _cashReserveInput = _config.CashReserve.Value;
            _scanIntervalInput = _config.ScanIntervalSeconds.Value;
            _pendingTimeoutInput = _config.PendingOrderTimeoutSeconds.Value;
            _buttonTextInput = _config.ButtonText.Value;
        }

        public void Draw()
        {
            if (!_visible) return;
            InitStyles();
            _windowRect = GUI.Window(_windowId, _windowRect, DrawWindowContents, "", _windowStyle);
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.12f, 0.95f));
            _windowStyle.onNormal.background = _windowStyle.normal.background;
            _windowStyle.padding = new RectOffset(12, 12, 8, 8);

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _headerStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            _sectionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            _sectionStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _labelStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

            _valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight
            };
            _valueStyle.normal.textColor = Color.white;

            _toggleStyle = new GUIStyle(GUI.skin.toggle) { fontSize = 13 };
            _toggleStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            _toggleStyle.onNormal.textColor = Color.white;
        }

        private void DrawWindowContents(int id)
        {
            GUILayout.Space(4f);
            GUILayout.Label("Order & Organize Settings", _headerStyle);
            GUILayout.Space(4f);

            float scrollHeight = _windowRect.height - 70f;
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(scrollHeight));

            DrawLine();

            // --- General ---
            GUILayout.Space(4f);
            GUILayout.Label("General", _sectionStyle);
            GUILayout.Space(4f);

            bool enabled = GUILayout.Toggle(_config.Enabled.Value, "  Mod Enabled", _toggleStyle);
            if (enabled != _config.Enabled.Value)
                _config.Enabled.Value = enabled;

            GUILayout.Space(4f);

            // Threshold slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Threshold:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label(_thresholdInput.ToString(), _valueStyle, GUILayout.Width(50f));
            GUILayout.EndHorizontal();
            float newThreshold = GUILayout.HorizontalSlider(_thresholdInput, 0f, 500f);
            _thresholdInput = Mathf.RoundToInt(newThreshold);
            if (_thresholdInput != _config.ThresholdUnits.Value)
                _config.ThresholdUnits.Value = _thresholdInput;

            GUILayout.Space(4f);

            // Button text
            GUILayout.BeginHorizontal();
            GUILayout.Label("Button Text:", _labelStyle, GUILayout.Width(120f));
            _buttonTextInput = GUILayout.TextField(_buttonTextInput, GUILayout.Width(180f));
            GUILayout.EndHorizontal();
            if (_buttonTextInput != _config.ButtonText.Value)
                _config.ButtonText.Value = _buttonTextInput;

            GUILayout.Space(2f);

            bool showNotif = GUILayout.Toggle(_config.ShowNotifications.Value, "  Show Notifications", _toggleStyle);
            if (showNotif != _config.ShowNotifications.Value)
                _config.ShowNotifications.Value = showNotif;

            bool verbose = GUILayout.Toggle(_config.VerboseLogging.Value, "  Verbose Logging", _toggleStyle);
            if (verbose != _config.VerboseLogging.Value)
                _config.VerboseLogging.Value = verbose;

            GUILayout.Space(8f);
            DrawLine();

            // --- Automation ---
            GUILayout.Space(4f);
            GUILayout.Label("Automation", _sectionStyle);
            GUILayout.Space(4f);

            bool autoStart = GUILayout.Toggle(_config.AutoOrderAtStartup.Value, "  Auto Order at Startup", _toggleStyle);
            if (autoStart != _config.AutoOrderAtStartup.Value)
                _config.AutoOrderAtStartup.Value = autoStart;

            GUILayout.Space(4f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Toggle Hotkey:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label(_config.ToggleHotkey.Value.ToString(), _valueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            // Scan interval slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scan Interval:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label($"{_scanIntervalInput}s", _valueStyle, GUILayout.Width(50f));
            GUILayout.EndHorizontal();
            float newInterval = GUILayout.HorizontalSlider(_scanIntervalInput, 2f, 300f);
            _scanIntervalInput = Mathf.RoundToInt(newInterval);
            if (_scanIntervalInput != _config.ScanIntervalSeconds.Value)
                _config.ScanIntervalSeconds.Value = _scanIntervalInput;

            GUILayout.Space(4f);

            // Cash reserve slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Cash Reserve:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label($"${_cashReserveInput:F0}", _valueStyle, GUILayout.Width(80f));
            GUILayout.EndHorizontal();
            float newReserve = GUILayout.HorizontalSlider(_cashReserveInput, 0f, 100000f);
            _cashReserveInput = Mathf.Round(newReserve / 100f) * 100f;
            if (Mathf.Abs(_cashReserveInput - _config.CashReserve.Value) > 1f)
                _config.CashReserve.Value = _cashReserveInput;

            GUILayout.Space(4f);

            // Pending timeout slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pending Timeout:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label($"{_pendingTimeoutInput}s", _valueStyle, GUILayout.Width(50f));
            GUILayout.EndHorizontal();
            float newTimeout = GUILayout.HorizontalSlider(_pendingTimeoutInput, 10f, 600f);
            _pendingTimeoutInput = Mathf.RoundToInt(newTimeout);
            if (_pendingTimeoutInput != _config.PendingOrderTimeoutSeconds.Value)
                _config.PendingOrderTimeoutSeconds.Value = _pendingTimeoutInput;

            GUILayout.Space(8f);
            DrawLine();
            GUILayout.Space(4f);

            // Automation status + toggle button
            bool isAutoOn = _getAutoEnabled?.Invoke() ?? false;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Auto Order:", _labelStyle, GUILayout.Width(120f));

            GUIStyle statusStyle = new GUIStyle(_valueStyle);
            statusStyle.normal.textColor = isAutoOn ? Color.green : new Color(1f, 0.4f, 0.4f);
            GUILayout.Label(isAutoOn ? "ACTIVE" : "OFF", statusStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            if (GUILayout.Button(isAutoOn ? "Disable Auto Order" : "Enable Auto Order", GUILayout.Height(30f)))
            {
                _toggleAuto?.Invoke();
            }

            GUILayout.Space(8f);
            DrawLine();

            // --- Category Shelves ---
            GUILayout.Space(4f);
            GUILayout.Label("Category Shelves", _sectionStyle);
            GUILayout.Space(4f);

            bool catEnabled = GUILayout.Toggle(_config.CategoryShelvesEnabled.Value, "  Category Shelves Enabled", _toggleStyle);
            if (catEnabled != _config.CategoryShelvesEnabled.Value)
                _config.CategoryShelvesEnabled.Value = catEnabled;

            GUILayout.Space(2f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Category Hotkey:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label(_config.CategoryHotkey.Value.ToString(), _valueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(2f);

            bool labelsVisible = GUILayout.Toggle(_config.CategoryLabelsVisible.Value, "  Show Floating Labels", _toggleStyle);
            if (labelsVisible != _config.CategoryLabelsVisible.Value)
                _config.CategoryLabelsVisible.Value = labelsVisible;

            GUILayout.Space(2f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Labels Hotkey:", _labelStyle, GUILayout.Width(120f));
            GUILayout.Label(_config.CategoryLabelsToggleHotkey.Value.ToString(), _valueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            GUILayout.EndScrollView();

            GUILayout.Space(4f);

            // Close button (fixed at bottom, outside scroll view)
            if (GUILayout.Button("Close", GUILayout.Height(25f)))
            {
                _visible = false;
            }

            GUI.DragWindow();
        }

        private void DrawLine()
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f, GUILayout.ExpandWidth(true));
            GUI.DrawTexture(rect, MakeLineTex(), ScaleMode.StretchToFill);
        }

        private static Texture2D _lineTex;
        private static Texture2D MakeLineTex()
        {
            if (_lineTex == null)
                _lineTex = MakeTex(1, 1, new Color(0.4f, 0.4f, 0.4f, 0.8f));
            return _lineTex;
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            var tex = new Texture2D(width, height);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}
