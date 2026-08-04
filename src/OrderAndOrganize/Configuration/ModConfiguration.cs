using BepInEx.Configuration;
using UnityEngine;

namespace OrderAndOrganize.Configuration
{
    public class ModConfiguration
    {
        public ConfigEntry<bool> Enabled { get; private set; }
        public ConfigEntry<int> ThresholdUnits { get; private set; }
        public ConfigEntry<string> ButtonText { get; private set; }
        public ConfigEntry<bool> VerboseLogging { get; private set; }

        public ConfigEntry<bool> AutoOrderAtStartup { get; private set; }
        public ConfigEntry<KeyCode> ToggleHotkey { get; private set; }
        public ConfigEntry<int> ScanIntervalSeconds { get; private set; }
        public ConfigEntry<float> CashReserve { get; private set; }
        public ConfigEntry<int> PendingOrderTimeoutSeconds { get; private set; }
        public ConfigEntry<bool> ShowNotifications { get; private set; }
        public ConfigEntry<KeyCode> ConfigWindowHotkey { get; private set; }

        public ConfigEntry<bool> CategoryShelvesEnabled { get; private set; }
        public ConfigEntry<KeyCode> CategoryHotkey { get; private set; }
        public ConfigEntry<bool> CategoryLabelsVisible { get; private set; }
        public ConfigEntry<KeyCode> CategoryLabelsToggleHotkey { get; private set; }

        public void Bind(ConfigFile config)
        {
            Enabled = config.Bind("General", "Enabled", true,
                "Enable or disable the Order & Organize mod.");

            ThresholdUnits = config.Bind("General", "ThresholdUnits", 40,
                new ConfigDescription(
                    "Products with CombinedStock below this value qualify for restocking.",
                    new AcceptableValueRange<int>(0, 9999)));

            ButtonText = config.Bind("General", "ButtonText", "Add Low Stock",
                "Text displayed on the manual restock button in the ordering interface.");

            VerboseLogging = config.Bind("General", "VerboseLogging", false,
                "Log per-product decisions to BepInEx log for debugging.");

            AutoOrderAtStartup = config.Bind("Automation", "AutoOrderAtStartup", false,
                "If true, automatic ordering starts enabled when the game loads.");

            ToggleHotkey = config.Bind("Automation", "ToggleHotkey", KeyCode.F8,
                "Key to toggle automatic ordering on/off.");

            ScanIntervalSeconds = config.Bind("Automation", "ScanIntervalSeconds", 10,
                new ConfigDescription(
                    "Seconds between automatic stock scans.",
                    new AcceptableValueRange<int>(2, 300)));

            CashReserve = config.Bind("Automation", "CashReserve", 0f,
                new ConfigDescription(
                    "Minimum money to keep in reserve. Automation will not spend below this amount.",
                    new AcceptableValueRange<float>(0f, 999999f)));

            PendingOrderTimeoutSeconds = config.Bind("Automation", "PendingOrderTimeoutSeconds", 120,
                new ConfigDescription(
                    "Seconds before a pending order record is considered stale and re-evaluated.",
                    new AcceptableValueRange<int>(10, 600)));

            ShowNotifications = config.Bind("Automation", "ShowNotifications", true,
                "Show in-game notifications for automation events.");

            ConfigWindowHotkey = config.Bind("General", "ConfigWindowHotkey", KeyCode.F7,
                "Key to toggle the in-game configuration window.");

            CategoryShelvesEnabled = config.Bind("CategoryShelves", "Enabled", true,
                "Enable the category storage shelf feature.");

            CategoryHotkey = config.Bind("CategoryShelves", "Hotkey", KeyCode.G,
                "Key to open the category picker while looking at a storage shelf.");

            CategoryLabelsVisible = config.Bind("CategoryShelves", "LabelsVisible", true,
                "Show floating category labels above tagged storage shelves.");

            CategoryLabelsToggleHotkey = config.Bind("CategoryShelves", "LabelsToggleHotkey", KeyCode.H,
                "Key to toggle floating category labels on/off.");
        }
    }
}
