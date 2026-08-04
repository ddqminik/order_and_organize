using OrderAndOrganize.Game;
using OrderAndOrganize.Models;

namespace OrderAndOrganize.Services
{
    public class NotificationService
    {
        private readonly GameNotificationAdapter _adapter;
        private readonly bool _enabled;

        public NotificationService(GameNotificationAdapter adapter, bool enabled)
        {
            _adapter = adapter;
            _enabled = enabled;
        }

        public void NotifyAutomationEnabled()
        {
            if (_enabled)
                _adapter.ShowNotification("Order & Organize automation enabled.");
        }

        public void NotifyAutomationDisabled()
        {
            if (_enabled)
                _adapter.ShowNotification("Order & Organize automation disabled.");
        }

        public void NotifyManualResult(int added, int skipped, int threshold)
        {
            if (!_enabled) return;

            if (added == 0 && skipped == 0)
            {
                _adapter.ShowNotification($"Order & Organize: nothing is below {threshold} units.");
                return;
            }

            string msg = $"Order & Organize: added {added} products to the shopping list.";
            if (skipped > 0)
                msg = $"Order & Organize: added {added} products, skipped {skipped} already listed.";

            _adapter.ShowNotification(msg);
        }

        public void NotifyAutomationResult(AutomationCycleResult result)
        {
            if (!_enabled) return;
            if (!result.HasPurchases && !result.HasSkips) return;

            if (result.ProductsPurchased > 0 && result.ProductsSkippedInsufficientFunds > 0)
            {
                _adapter.ShowImportantNotification(
                    $"Auto-Order: {result.ProductsPurchased} boxes purchased for ${result.TotalSpent:F2} " +
                    $"({result.ProductsSkippedInsufficientFunds} skipped - low funds)");
            }
            else if (result.ProductsPurchased > 0)
            {
                _adapter.ShowImportantNotification(
                    $"Auto-Order: {result.ProductsPurchased} boxes purchased for ${result.TotalSpent:F2}");
            }
        }

        public void NotifyManualListBlocking()
        {
            if (_enabled)
                _adapter.ShowRateLimitedNotification("manual_list_block",
                    "Order & Organize paused: manual shopping list is not empty.");
        }

        public void NotifyNotAuthority()
        {
            if (_enabled)
                _adapter.ShowRateLimitedNotification("not_authority",
                    "Order & Organize: automation disabled (not host).");
        }

        public void NotifyCompatibilityError(string detail)
        {
            _adapter.ShowNotification(
                $"Order & Organize could not access the current ordering system. Check BepInEx logs.");
        }
    }
}
