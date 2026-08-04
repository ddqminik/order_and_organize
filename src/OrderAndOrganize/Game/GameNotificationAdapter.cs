using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace OrderAndOrganize.Game
{
    public class GameNotificationAdapter
    {
        private readonly ManualLogSource _log;
        private readonly Dictionary<string, DateTime> _lastNotificationTimes = new Dictionary<string, DateTime>();
        private readonly TimeSpan _rateLimitInterval = TimeSpan.FromSeconds(10);
        private FieldInfo _inCooldownField;
        private bool _cooldownFieldResolved;

        public GameNotificationAdapter(ManualLogSource log)
        {
            _log = log;
        }

        private void ResolveCooldownField()
        {
            if (_cooldownFieldResolved) return;
            _inCooldownField = typeof(GameCanvas).GetField("inCooldown",
                BindingFlags.Instance | BindingFlags.NonPublic);
            _cooldownFieldResolved = true;
        }

        private void ClearCooldownIfNeeded()
        {
            ResolveCooldownField();
            if (_inCooldownField != null && GameCanvas.Instance != null)
            {
                _inCooldownField.SetValue(GameCanvas.Instance, false);
            }
        }

        /// <summary>
        /// Send a notification using the game's native notification system.
        /// Messages are prefixed with backtick to bypass localization lookup.
        /// </summary>
        public void ShowNotification(string message)
        {
            if (GameCanvas.Instance == null)
            {
                _log.LogInfo($"[Notification] {message}");
                return;
            }

            try
            {
                ClearCooldownIfNeeded();
                GameCanvas.Instance.CreateCanvasNotification("`" + message);
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Failed to show notification: {ex.Message}");
                _log.LogInfo($"[Notification] {message}");
            }
        }

        /// <summary>
        /// Send a prominent notification using the game's important notification system.
        /// </summary>
        public void ShowImportantNotification(string message)
        {
            if (GameCanvas.Instance == null)
            {
                _log.LogInfo($"[Important] {message}");
                return;
            }

            try
            {
                ClearCooldownIfNeeded();
                GameCanvas.Instance.CreateImportantNotification("`" + message);
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Failed to show important notification, falling back: {ex.Message}");
                ShowNotification(message);
            }
        }

        /// <summary>
        /// Show a notification with rate limiting for repeated warnings.
        /// </summary>
        public void ShowRateLimitedNotification(string key, string message)
        {
            if (_lastNotificationTimes.TryGetValue(key, out var lastTime))
            {
                if (DateTime.UtcNow - lastTime < _rateLimitInterval)
                    return;
            }

            ShowNotification(message);
            _lastNotificationTimes[key] = DateTime.UtcNow;
        }
    }
}
